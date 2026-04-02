using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using EuskalIA.Server.Models;
using EuskalIA.Server.Services.Auth;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace EuskalIA.Tests.Services
{
    public class JwtServiceTests
    {
        private readonly IConfiguration _configuration;
        private readonly JwtService _jwtService;

        public JwtServiceTests()
        {
            var inMemorySettings = new Dictionary<string, string> {
                {"JwtSettings:Secret", "TestSecretKey12345678901234567890"},
                {"JwtSettings:Issuer", "EuskalIA"},
                {"JwtSettings:Audience", "EuskalIAUsers"},
                {"JwtSettings:ExpiryMinutes", "60"}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            var mockLogger = new Mock<ILogger<JwtService>>();
            _jwtService = new JwtService(_configuration, mockLogger.Object);
        }

        [Fact]
        public void GenerateToken_ShouldReturnValidJwt()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "testuser",
                Role = "User"
            };

            // Act
            var token = _jwtService.GenerateToken(user);

            // Assert
            Assert.NotNull(token);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            Assert.Equal("EuskalIA", jwtToken.Issuer);
            Assert.Contains(jwtToken.Audiences, a => a == "EuskalIAUsers");
            Assert.Equal("1", jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub || c.Type == ClaimTypes.NameIdentifier).Value);
            Assert.Equal("testuser", jwtToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Name || c.Type == ClaimTypes.Name).Value);
            Assert.Equal("User", jwtToken.Claims.First(c => c.Type == "role" || c.Type == ClaimTypes.Role).Value);
        }

        [Fact]
        public void GenerateToken_ShouldIncludeAdminRole_WhenUserIsAdmin()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "adminuser",
                Role = "Admin"
            };

            // Act
            var token = _jwtService.GenerateToken(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            Assert.Equal("Admin", jwtToken.Claims.First(c => c.Type == "role" || c.Type == ClaimTypes.Role).Value);
        }
    }
}
