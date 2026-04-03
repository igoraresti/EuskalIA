using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EuskalIA.Server.Models;
using Microsoft.IdentityModel.Tokens;

namespace EuskalIA.Server.Services.Auth
{
    /// <summary>
    /// Implementation of <see cref="IJwtService"/> that issues tokens based on ASP.NET Coro Identity model.
    /// Manages configuration retrieval for secret keys, issuers, and audiences.
    /// </summary>
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtService"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration provider.</param>
        /// <param name="logger">The service logger.</param>
        public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Generates a signed JWT token for the specified user containing their identity, username, and role.
        /// Validates configuration before generating security keys.
        /// </summary>
        /// <param name="user">The user for whom to generate the token.</param>
        /// <returns>A signed JWT token string.</returns>
        public string GenerateToken(User user)
        {
            _logger.LogInformation("Generating JWT token for user {Username} (ID: {UserId}, Role: {Role}).", user.Username, user.Id, user.Role);
            var secret = _configuration["JwtSettings:Secret"]!;
            var issuer = _configuration["JwtSettings:Issuer"]!;
            var audience = _configuration["JwtSettings:Audience"]!;
            var expiryMinutes = _configuration.GetValue<int>("JwtSettings:ExpiryMinutes", 1440);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
