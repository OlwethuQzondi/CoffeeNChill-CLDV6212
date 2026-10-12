using System.Text.Json.Serialization;

namespace CoffeeNChillFunctions.DTOs
{
    public class OrderMessageDto
    {
        [JsonPropertyName("orderId")]
        public string OrderId { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("customerName")]
        public string CustomerName { get; set; } = string.Empty;

        [JsonPropertyName("coffeeItem")]
        public string CoffeeItem { get; set; } = string.Empty;

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; } = 1;
    }
}