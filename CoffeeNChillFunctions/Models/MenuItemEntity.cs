using Azure;
using Azure.Data.Tables;
using System;

namespace CoffeeNChillFunctions.Models
{
    public class MenuItemEntity : ITableEntity
    {
        public string PartitionKey { get; set; } // Category e.g. "Hot Drinks", "Pastries"
        public string RowKey { get; set; }       // Unique SKU/ID e.g. "COF-001"
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public bool IsAvailable { get; set; }

        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}