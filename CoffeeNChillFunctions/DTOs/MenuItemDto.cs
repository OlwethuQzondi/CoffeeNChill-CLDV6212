using System;
using System.Text.Json.Serialization;

namespace CoffeeNChillFunctions.DTOs
{
    public class MenuItemDto
    {
        [JsonPropertyName("category")]
        public string Category { get; set; } = string.Empty;

        [JsonPropertyName("sku")]
        public string Sku { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("price")]
        public double Price { get; set; }

        [JsonPropertyName("isAvailable")]
        public bool IsAvailable { get; set; }
    }

    public class UpdateMenuItemDto
    {
        [JsonPropertyName("price")]
        public double Price { get; set; }

        [JsonPropertyName("isAvailable")]
        public bool IsAvailable { get; set; }
    }

    public class StaffDocumentDto
    {
        [JsonPropertyName("fileName")]
        public string FileName { get; set; } = string.Empty;

        [JsonPropertyName("sizeInBytes")]
        public long SizeInBytes { get; set; }

        [JsonPropertyName("contentType")]
        public string ContentType { get; set; } = string.Empty;

        [JsonPropertyName("lastModified")]
        public DateTimeOffset? LastModified { get; set; }
    }
}