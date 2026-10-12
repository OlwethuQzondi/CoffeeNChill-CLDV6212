/*
 * IEEE Citation:
 * [1] Microsoft, "Azure Functions Azure Queue storage output binding," Microsoft Learn, 2024. 
 *     Available: https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-queue-output
 * [2] Microsoft, "Work with Azure Storage queues in .NET," Microsoft Learn, 2023.
 *     Available: https://learn.microsoft.com/en-us/azure/storage/queues/storage-dotnet-how-to-use-queues
 */

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using CoffeeNChillFunctions.DTOs;

namespace CoffeeNChillFunctions
{
    public class OrderQueueFunctions
    {
        private readonly ILogger<OrderQueueFunctions> _logger;

        public OrderQueueFunctions(ILogger<OrderQueueFunctions> logger)
        {
            _logger = logger;
        }

        [Function("EnqueueOrder")]
        public async Task<MultiResponse> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "orders/queue")] HttpRequestData req)
        {
            _logger.LogInformation("Processing order enqueue request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var orderData = JsonSerializer.Deserialize<OrderDto>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (orderData == null || string.IsNullOrEmpty(orderData.OrderId) || orderData.TotalPrice <= 0)
            {
                var badResponse = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Invalid order payload.");
                return new MultiResponse { HttpResponse = badResponse, OutputQueueMessage = string.Empty };
            }

            if (string.IsNullOrEmpty(orderData.OrderTimestamp))
            {
                orderData.OrderTimestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
            }

            string jsonMessage = JsonSerializer.Serialize(orderData);

            var okResponse = req.CreateResponse(System.Net.HttpStatusCode.Accepted);
            await okResponse.WriteAsJsonAsync(new { message = "Order successfully placed into queue.", orderId = orderData.OrderId });

            return new MultiResponse
            {
                HttpResponse = okResponse,
                OutputQueueMessage = jsonMessage
            };
        }
    }

    public class MultiResponse
    {
        [HttpResult]
        public HttpResponseData HttpResponse { get; set; } = null!;

        [QueueOutput("order-processing-queue", Connection = "AzureWebJobsStorage")]
        public string OutputQueueMessage { get; set; } = string.Empty;
    }
}