// Developed by Olwethu Zondi & Aphiwe Ndlovu - CoffeeNChill Systems
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