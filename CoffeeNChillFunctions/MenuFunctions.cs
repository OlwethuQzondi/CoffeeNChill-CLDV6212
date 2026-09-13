using Azure;
using Azure.Data.Tables;
using CoffeeNChillFunctions.DTOs;
using CoffeeNChillFunctions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace CoffeeNChillFunctions
{
    public class MenuFunctions
    {
        private readonly ILogger<MenuFunctions> _logger;
        private readonly TableClient _tableClient;

        public MenuFunctions(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<MenuFunctions>();
            string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage") ?? "UseDevelopmentStorage=true";
            TableServiceClient serviceClient = new TableServiceClient(connectionString);
            _tableClient = serviceClient.GetTableClient("MenuItems");
            _tableClient.CreateIfNotExists();
        }

        // 1. CREATE Menu Item
        [Function("CreateMenuItem")]
        public async Task<HttpResponseData> CreateMenuItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "menu")] HttpRequestData req)
        {
            _logger.LogInformation("Creating a new menu item.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var dto = JsonSerializer.Deserialize<MenuItemDto>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (dto == null || string.IsNullOrWhiteSpace(dto.Category) || string.IsNullOrWhiteSpace(dto.Sku))
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteStringAsync("Invalid payload. Category and Sku are required.");
                return badResponse;
            }

            var entity = new MenuItemEntity
            {
                PartitionKey = dto.Category,
                RowKey = dto.Sku,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                IsAvailable = dto.IsAvailable
            };

            await _tableClient.UpsertEntityAsync(entity);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(entity);
            return response;
        }

        // 2. READ ALL Menu Items
        [Function("GetAllMenuItems")]
        public async Task<HttpResponseData> GetAllMenuItems(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "menu")] HttpRequestData req)
        {
            _logger.LogInformation("Retrieving all menu items.");
            var items = new List<MenuItemEntity>();

            await foreach (var item in _tableClient.QueryAsync<MenuItemEntity>())
            {
                items.Add(item);
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(items);
            return response;
        }

        // 3. READ Menu Items BY CATEGORY
        [Function("GetMenuItemsByCategory")]
        public async Task<HttpResponseData> GetMenuItemsByCategory(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "menu/category/{category}")] HttpRequestData req,
            string category)
        {
            _logger.LogInformation($"Retrieving menu items for category: {category}");
            var items = new List<MenuItemEntity>();

            await foreach (var item in _tableClient.QueryAsync<MenuItemEntity>(e => e.PartitionKey == category))
            {
                items.Add(item);
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(items);
            return response;
        }

        // 4. UPDATE Menu Item
        [Function("UpdateMenuItem")]
        public async Task<HttpResponseData> UpdateMenuItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "menu/{category}/{sku}")] HttpRequestData req,
            string category,
            string sku)
        {
            _logger.LogInformation($"Updating menu item {sku} in category {category}");

            try
            {
                var existingEntity = await _tableClient.GetEntityAsync<MenuItemEntity>(category, sku);
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var dto = JsonSerializer.Deserialize<UpdateMenuItemDto>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (dto != null)
                {
                    existingEntity.Value.Price = dto.Price;
                    existingEntity.Value.IsAvailable = dto.IsAvailable;
                    await _tableClient.UpdateEntityAsync(existingEntity.Value, ETag.All, TableUpdateMode.Replace);
                }

                var response = req.CreateResponse(HttpStatusCode.OK);
                await response.WriteAsJsonAsync(existingEntity.Value);
                return response;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync("Menu item not found.");
                return notFoundResponse;
            }
        }

        // 5. DELETE Menu Item
        [Function("DeleteMenuItem")]
        public async Task<HttpResponseData> DeleteMenuItem(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "menu/{category}/{sku}")] HttpRequestData req,
            string category,
            string sku)
        {
            _logger.LogInformation($"Deleting menu item {sku} from category {category}");

            try
            {
                await _tableClient.DeleteEntityAsync(category, sku);
                var response = req.CreateResponse(HttpStatusCode.NoContent);
                return response;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                var notFoundResponse = req.CreateResponse(HttpStatusCode.NotFound);
                await notFoundResponse.WriteStringAsync("Menu item not found.");
                return notFoundResponse;
            }
        }
    }
}