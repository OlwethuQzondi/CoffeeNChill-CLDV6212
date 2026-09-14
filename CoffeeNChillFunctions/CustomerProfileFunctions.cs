using System.Net;
using System.Text.Json;
using Azure.Data.Tables;
using CoffeeNChillFunctions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CoffeeNChillFunctions
{
    public class CustomerProfileFunctions
    {
        private readonly TableServiceClient _tableServiceClient;
        private readonly ILogger<CustomerProfileFunctions> _logger;
        private const string TableName = "CustomerProfiles";

        public CustomerProfileFunctions(IConfiguration configuration, ILogger<CustomerProfileFunctions> logger)
        {
            _logger = logger;
            var connectionString = configuration["AzureWebJobsStorage"];
            _tableServiceClient = new TableServiceClient(connectionString);
        }

        [Function("CreateCustomerProfile")]
        public async Task<HttpResponseData> CreateProfile(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "profiles")] HttpRequestData req)
        {
            _logger.LogInformation("Processing request to create a customer profile.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var profile = JsonSerializer.Deserialize<CustomerProfileEntity>(requestBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (profile == null || string.IsNullOrWhiteSpace(profile.Name))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Invalid profile payload.");
                return badResponse;
            }

            if (string.IsNullOrEmpty(profile.RowKey))
            {
                profile.RowKey = Guid.NewGuid().ToString();
            }
            profile.PartitionKey = "Customer";

            var tableClient = _tableServiceClient.GetTableClient(TableName);
            await tableClient.CreateIfNotExistsAsync();
            await tableClient.UpsertEntityAsync(profile);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(profile);
            return response;
        }

        [Function("GetCustomerProfiles")]
        public async Task<HttpResponseData> GetProfiles(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "profiles")] HttpRequestData req)
        {
            _logger.LogInformation("Fetching all customer profiles.");

            var tableClient = _tableServiceClient.GetTableClient(TableName);
            await tableClient.CreateIfNotExistsAsync();

            var profiles = new List<CustomerProfileEntity>();
            await foreach (var entity in tableClient.QueryAsync<CustomerProfileEntity>())
            {
                profiles.Add(entity);
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(profiles);
            return response;
        }
    }
}