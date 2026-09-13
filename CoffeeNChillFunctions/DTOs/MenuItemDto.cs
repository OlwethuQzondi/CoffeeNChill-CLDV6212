namespace CoffeeNChillFunctions.DTOs
{
    public class MenuItemDto
    {
        public string Category { get; set; }
        public string Sku { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class UpdateMenuItemDto
    {
        public double Price { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class StaffDocumentDto
    {
        public string FileName { get; set; }
        public long SizeInBytes { get; set; }
        public string ContentType { get; set; }
        public System.DateTimeOffset? LastModified { get; set; }
    }
}