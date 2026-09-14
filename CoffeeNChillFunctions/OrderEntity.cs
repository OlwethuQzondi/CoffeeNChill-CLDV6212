// Developed jointly by Olwethu & Aphiwe
using Azure;
using Azure.Data.Tables;
using System;

namespace CoffeeNChillFunctions.Models
{
    public class OrderEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = string.Empty;
        public string RowKey { get; set; } = string.Empty;
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string CustomerName { get; set; } = string.Empty;
        public double TotalPrice { get; set; }
        public string Status { get; set; } = "Received";
        public string OrderTimestamp { get; set; } = string.Empty;
    }
}