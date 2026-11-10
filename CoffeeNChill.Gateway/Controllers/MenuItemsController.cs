using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeNChill.Gateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuItemsController : ControllerBase
    {
        // Static in-memory mock or connected storage for demonstration
        private static readonly List<object> _menuItems = new()
        {
            new { Id = 1, Name = "Caramel Macchiato", Price = 45.00, Description = "Sweet espresso with vanilla and caramel drizzle" },
            new { Id = 2, Name = "Cold Brew Chill", Price = 40.00, Description = "Smooth slow-steeped cold coffee over ice" },
            new { Id = 3, Name = "Classic Cappuccino", Price = 38.00, Description = "Rich espresso with steamed milk foam" }
        };

        // Public or Authenticated Users (Customers & Staff) can view the menu
        [HttpGet]
        [Authorize]
        public IActionResult GetMenu()
        {
            return Ok(_menuItems);
        }

        // Only users with the "Staff" role can add new menu items
        [HttpPost]
        [Authorize(Roles = "Staff")]
        public IActionResult AddMenuItem([FromBody] MenuItemCreateDto dto)
        {
            var newItem = new
            {
                Id = _menuItems.Count + 1,
                Name = dto.Name,
                Price = dto.Price,
                Description = dto.Description
            };

            _menuItems.Add(newItem);
            return StatusCode(201, new { message = "Menu item added successfully by Staff!", item = newItem });
        }
    }

    public record MenuItemCreateDto(string Name, double Price, string Description);
}