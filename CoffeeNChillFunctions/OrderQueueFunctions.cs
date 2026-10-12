using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CoffeeNChillFunctions.DTOs;

namespace CoffeeNChillFunctions
{
    public class OrderQueueFunctions
    {
        private readonly QueueServiceClient _queueServiceClient;
        private readonly ILogger<OrderQueueFunctions> _logger;
        private const string QueueName = "order-processing-queue";

        {
            _logger = logger;
            var connectionString = configuration["AzureWebJobsStorage"];
            _queueServiceClient = new QueueServiceClient(connectionString);
        }

        {

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            {

            {
            }



        }

        {
    }
}