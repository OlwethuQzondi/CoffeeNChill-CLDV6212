using Azure;
using Azure.Data.Tables;

namespace CoffeeNChill.Gateway.Models
{
    public class UserProfileEntity : ITableEntity
    {
        public string PartitionKey { get; set; } = "USER";
        public string RowKey { get; set; } = default!; // Email address as unique identifier
        public string Username { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public string Role { get; set; } = "Customer"; // RBAC Role: "Customer" or "Staff"
        public ETag ETag { get; set; } = ETag.All;
        public DateTimeOffset? Timestamp { get; set; }
    }
}