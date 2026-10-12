using Azure;
using Azure.Data.Tables;
using Azure.Storage.Queues;
using CoffeeNChillFunctions.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class OrderQueueFunctions
{
    private readonly ILogger<OrderQueueFunctions> _logger;
    private readonly string _storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");

    public OrderQueueFunctions(ILogger<OrderQueueFunctions> logger)
    {
        _logger = logger;
    }

    [Function("SendOrderToQueue")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders")] HttpRequest req)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var order = JsonSerializer.Deserialize<OrderMessageDto>(requestBody);

        if (order == null || string.IsNullOrWhiteSpace(order.CoffeeItem))
        {
            return new BadRequestObjectResult("Invalid order details.");
        }

        // 1. Save to Table Storage using OrderId as both PartitionKey and RowKey for instant indexing
        var tableClient = new TableClient(_storageConnectionString, "CoffeeOrders");
        await tableClient.CreateIfNotExistsAsync();

        var entity = new TableEntity(order.OrderId, order.OrderId)
        {
            { "CustomerName", order.CustomerName },
            { "CoffeeItem", order.CoffeeItem },
            { "Quantity", order.Quantity },
            { "Status", "Order received" }, // Initial barista lifecycle status
            { "OrderDate", DateTime.UtcNow }
        };
        await tableClient.UpsertEntityAsync(entity);

        // 2. Base64 encode queue payload
        var queueClient = new QueueClient(_storageConnectionString, "order-queue");
        await queueClient.CreateIfNotExistsAsync();

        string jsonPayload = JsonSerializer.Serialize(order);
        byte[] plainTextBytes = Encoding.UTF8.GetBytes(jsonPayload);
        string base64Payload = Convert.ToBase64String(plainTextBytes);
        await queueClient.SendMessageAsync(base64Payload);

        _logger.LogInformation($"Order {order.OrderId} stored in table and queued via Base64.");

        return new OkObjectResult(new { status = "Order queued successfully", orderId = order.OrderId });
    }

    [Function("UpdateOrderStatus")]
    public async Task<IActionResult> UpdateStatus(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "orders/{orderId}/status")] HttpRequest req,
        string orderId)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var data = JsonSerializer.Deserialize<JsonElement>(requestBody);

        if (!data.TryGetProperty("status", out var statusProp) || string.IsNullOrWhiteSpace(statusProp.GetString()))
        {
            return new BadRequestObjectResult("Status property is required.");
        }

        string newStatus = statusProp.GetString();
        var tableClient = new TableClient(_storageConnectionString, "CoffeeOrders");

        await tableClient.CreateIfNotExistsAsync();

        try
        {
            // Direct point-lookup using PartitionKey & RowKey (Fastest & direct)
            var response = await tableClient.GetEntityAsync<TableEntity>(orderId, orderId);
            var ent = response.Value;

            ent["Status"] = newStatus;
            await tableClient.UpdateEntityAsync(ent, ETag.All, TableUpdateMode.Replace);

            return new OkObjectResult(new { status = "Order status updated successfully", orderId = orderId, newStatus = newStatus });
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return new NotFoundObjectResult(new { error = $"Order with ID '{orderId}' not found." });
        }
    }
}