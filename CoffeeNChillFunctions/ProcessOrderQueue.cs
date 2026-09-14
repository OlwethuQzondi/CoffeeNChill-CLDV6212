// Developed by Aphiwe Ndlovu (Aphiwe's VM)
using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Data.Tables;
using CoffeeNChillFunctions.DTOs;
using CoffeeNChillFunctions.Models;

namespace CoffeeNChillFunctions
{
    public class ProcessOrderQueue
    {
        private readonly ILogger<ProcessOrderQueue> _logger;
        private const string TableName = "Orders";

        public ProcessOrderQueue(ILogger<ProcessOrderQueue> logger)
        {
            _logger = logger;
        }

        [Function("ProcessOrderQueue")]
        public async Task Run(
            [QueueTrigger("order-processing-queue", Connection = "AzureWebJobsStorage")] string queueMessage)
        {
            _logger.LogInformation($"[QueueTrigger] Received order message: {queueMessage}");

            var order = JsonConvert.DeserializeObject<OrderDto>(queueMessage);
            if (order == null)
            {
                _logger.LogError("Failed to deserialize queue message into OrderDto.");
                return;
            }

            string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage") ?? "UseDevelopmentStorage=true";
            var tableClient = new TableClient(connectionString, TableName);
            await tableClient.CreateIfNotExistsAsync();

            string partitionKey = DateTime.UtcNow.ToString("yyyy-MM-dd");
            string rowKey = order.OrderId;

            // 1. Status: Received
            var orderEntity = new OrderEntity
            {
                PartitionKey = partitionKey,
                RowKey = rowKey,
                CustomerName = order.CustomerName,
                TotalPrice = order.TotalPrice,
                Status = "Received",
                OrderTimestamp = order.OrderTimestamp
            };

            await tableClient.UpsertEntityAsync(orderEntity);
            _logger.LogInformation($"Order {order.OrderId} status updated to: Received");

            // Simulate Barista Lifecycle Transitions
            await Task.Delay(1000);
            orderEntity.Status = "Preparing";
            await tableClient.UpsertEntityAsync(orderEntity);
            _logger.LogInformation($"Order {order.OrderId} status updated to: Preparing");

            await Task.Delay(1000);
            orderEntity.Status = "Ready";
            await tableClient.UpsertEntityAsync(orderEntity);
            _logger.LogInformation($"Order {order.OrderId} status updated to: Ready");

            await Task.Delay(1000);
            orderEntity.Status = "Collected";
            await tableClient.UpsertEntityAsync(orderEntity);
            _logger.LogInformation($"Order {order.OrderId} status updated to: Collected");
        }
    }
}