using Azure;
using Azure.Data.Tables;
using CoffeeNChill.Gateway.DTOs;
using CoffeeNChill.Gateway.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CoffeeNChill.Gateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TableClient _userTableClient;
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
            string connectionString = _configuration["AzureStorage:ConnectionString"] ?? "UseDevelopmentStorage=true";
            var serviceClient = new TableServiceClient(connectionString);
            _userTableClient = serviceClient.GetTableClient("UserProfiles");
            _userTableClient.CreateIfNotExists();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                var existing = await _userTableClient.GetEntityAsync<UserProfileEntity>("USER", dto.Email);
                if (existing.Value != null)
                    return BadRequest(new { message = "User with this email already exists." });
            }
            catch (RequestFailedException) { /* User does not exist, safe to proceed */ }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var userEntity = new UserProfileEntity
            {
                PartitionKey = "USER",
                RowKey = dto.Email,
                Username = dto.Username,
                PasswordHash = passwordHash,
                Role = string.IsNullOrEmpty(dto.Role) ? "Customer" : dto.Role
            };

            await _userTableClient.AddEntityAsync(userEntity);

            return Ok(new { message = "User registered successfully!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            UserProfileEntity user;
            try
            {
                var response = await _userTableClient.GetEntityAsync<UserProfileEntity>("USER", dto.Email);
                user = response.Value;
            }
            catch (RequestFailedException)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            bool verified = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!verified)
                return Unauthorized(new { message = "Invalid email or password." });

            string token = GenerateJwtToken(user);

            return Ok(new AuthResponseDto(token, user.Username, user.RowKey, user.Role));
        }

        private string GenerateJwtToken(UserProfileEntity user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var secretKey = jwtSettings["Key"] ?? "SuperSecretKeyDefaultForDevelopmentOnly12345!";
            var issuer = jwtSettings["Issuer"] ?? "CoffeeNChillGateway";
            var audience = jwtSettings["Audience"] ?? "CoffeeNChillUsers";

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.RowKey),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.RowKey),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(3),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}