/*
 * IEEE Citation:
 * [1] Microsoft, "Azure Functions Azure Queue storage output binding," Microsoft Learn, 2024. 
 *     Available: https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-queue-output
 * [2] Microsoft, "Work with Azure Storage queues in .NET," Microsoft Learn, 2023.
 *     Available: https://learn.microsoft.com/en-us/azure/storage/queues/storage-dotnet-how-to-use-queues
 */
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CoffeeNChillFunctions.DTOs
{
    public class OrderDto
    {
        [JsonPropertyName("orderId")]
        public string OrderId { get; set; } = string.Empty;

        [JsonPropertyName("customerName")]
        public string CustomerName { get; set; } = string.Empty;

        [JsonPropertyName("selectedItemSKUs")]
        public List<string> SelectedItemSKUs { get; set; } = new();

        [JsonPropertyName("totalPrice")]
        public double TotalPrice { get; set; }

        [JsonPropertyName("orderTimestamp")]
        public string OrderTimestamp { get; set; } = string.Empty;
    }
}