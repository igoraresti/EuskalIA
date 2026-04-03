using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EuskalIA.Server.Data;
using EuskalIA.Server.Models;
using EuskalIA.Server.DTOs.Auth;
using EuskalIA.Server.Services.Auth;
using EuskalIA.Server.Services.Encryption;

namespace EuskalIA.Server.Controllers
{
    /// <summary>
    /// Controller for handling authentication and social login operations.
    /// </summary>
    [ApiController]
    [Route("api/euskalia/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ISocialAuthService _socialAuthService;
        private readonly IJwtService _jwtService;
        private readonly IEncryptionService _encryptionService;
        private readonly ILogger<AuthController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="socialAuthService">The social authentication service.</param>
        /// <param name="jwtService">The JWT token service.</param>
        /// <param name="encryptionService">The data encryption service.</param>
        /// <param name="logger">The controller logger.</param>
        public AuthController(
            AppDbContext context,
            ISocialAuthService socialAuthService,
            IJwtService jwtService,
            IEncryptionService encryptionService,
            ILogger<AuthController> logger)
        {
            _context = context;
            _socialAuthService = socialAuthService;
            _jwtService = jwtService;
            _encryptionService = encryptionService;
            _logger = logger;
        }

        /// <summary>
        /// Authenticates a user via a social provider (Google or Facebook).
        /// If the user does not exist, a new profile is created automatically.
        /// </summary>
        /// <param name="socialLoginDto">The social login payload containing the provider and token.</param>
        /// <returns>An <see cref="IActionResult"/> containing the JWT token and user profile if successful.</returns>
        [HttpPost("social-login")]
        public async Task<IActionResult> SocialLogin([FromBody] SocialLoginDto socialLoginDto)
        {
            _logger.LogInformation("Social login attempt with provider {Provider}.", socialLoginDto.Provider);
            User? socialUser = null;

            if (socialLoginDto.Provider.ToLower() == "google")
            {
                socialUser = await _socialAuthService.ValidateGoogleTokenAsync(socialLoginDto.Token);
            }
            else if (socialLoginDto.Provider.ToLower() == "facebook")
            {
                socialUser = await _socialAuthService.ValidateFacebookTokenAsync(socialLoginDto.Token);
            }

            if (socialUser == null)
            {
                _logger.LogWarning("Social login failed: Invalid token for provider {Provider}.", socialLoginDto.Provider);
                return Unauthorized(new { message = "Invalid social token" });
            }

            // Find user by email (encrypted in DB)
            var allUsers = await _context.Users.ToListAsync();
            var user = allUsers.FirstOrDefault(u => _encryptionService.Decrypt(u.Email) == socialUser.Email);

            if (user == null)
            {
                _logger.LogInformation("Creating new user from social login: {Email}.", socialUser.Email);
                // Create unique nickname (Name-XXXX)
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                string generatedNickname;
                bool isUnique = false;
                
                // Clean the base nickname (remove spaces, etc). If empty, use "User"
                string baseNickname = string.IsNullOrWhiteSpace(socialUser.Nickname) ? "User" : socialUser.Nickname.Replace(" ", "");
                
                do {
                    var suffix = new string(Enumerable.Repeat(chars, 4).Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
                    generatedNickname = $"{baseNickname}-{suffix}";
                    var encryptedGenNick = _encryptionService.Encrypt(generatedNickname);
                    isUnique = !await _context.Users.AnyAsync(u => u.Nickname == encryptedGenNick);
                } while (!isUnique);

                // Create new user mapping Name->Username, and Generated->Nickname
                user = new User
                {
                    Username = string.IsNullOrWhiteSpace(socialUser.Nickname) ? "Usuario" : socialUser.Nickname,
                    Email = _encryptionService.Encrypt(socialUser.Email),
                    Nickname = _encryptionService.Encrypt(generatedNickname),
                    Password = _encryptionService.Encrypt(Guid.NewGuid().ToString()), // Random password
                    JoinedAt = DateTime.UtcNow,
                    IsVerified = true,
                    IsActive = true,
                    Language = socialLoginDto.Language ?? "es",
                    Role = "User"
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully created user {Username} (ID: {UserId}) via social login.", user.Username, user.Id);
            }
            else if (!user.IsActive)
            {
                _logger.LogWarning("Social login attempt for deactivated user {Username} (ID: {UserId}).", user.Username, user.Id);
                return Unauthorized(new { message = "Account deactivated" });
            }

            _logger.LogInformation("Social login successful for user {Username} (ID: {UserId}).", user.Username, user.Id);

            var token = _jwtService.GenerateToken(user);

            return Ok(new
            {
                Token = token,
                User = new
                {
                    user.Id,
                    user.Username,
                    Nickname = _encryptionService.Decrypt(user.Nickname),
                    Email = _encryptionService.Decrypt(user.Email),
                    user.JoinedAt,
                    user.Language,
                    user.Role
                }
            });
        }
    }
}
