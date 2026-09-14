using System.Net;
using System.Text.Json;
using Azure.Storage.Queues;
using CoffeeNChillFunctions.DTOs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CoffeeNChillFunctions
{
    public class OrderQueueFunctions
    {
        private readonly QueueServiceClient _queueServiceClient;
        private readonly ILogger<OrderQueueFunctions> _logger;
        private const string QueueName = "order-processing-queue";

        public OrderQueueFunctions(IConfiguration configuration, ILogger<OrderQueueFunctions> logger)
        {
            _logger = logger;
            var connectionString = configuration["AzureWebJobsStorage"];
            _queueServiceClient = new QueueServiceClient(connectionString);
        }

        [Function("SendOrderToQueue")]
        public async Task<HttpResponseData> SendOrder(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "orders")] HttpRequestData req)
        {
            _logger.LogInformation("Receiving order payload to enqueue.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var order = JsonSerializer.Deserialize<OrderMessageDto>(requestBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (order == null || string.IsNullOrWhiteSpace(order.CoffeeItem))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Invalid order details.");
                return badResponse;
            }

            var queueClient = _queueServiceClient.GetQueueClient(QueueName);
            await queueClient.CreateIfNotExistsAsync();

            string messageText = JsonSerializer.Serialize(order);
            await queueClient.SendMessageAsync(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(messageText)));

            var response = req.CreateResponse(HttpStatusCode.Accepted);
            await response.WriteAsJsonAsync(new { status = "Order queued successfully", orderId = order.OrderId });
            return response;
        }

        [Function("ProcessOrderQueue")]
        public void ProcessOrder(
            [QueueTrigger(QueueName, Connection = "AzureWebJobsStorage")] string messageText)
        {
            _logger.LogInformation($"[Queue Consumer] Processing background order message: {messageText}");
        }
    }
}