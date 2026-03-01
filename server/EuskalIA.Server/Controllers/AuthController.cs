using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EuskalIA.Server.Data;
using EuskalIA.Server.Models;
using EuskalIA.Server.DTOs;
using EuskalIA.Server.Services.Auth;
using EuskalIA.Server.Services.Encryption;

namespace EuskalIA.Server.Controllers
{
    [ApiController]
    [Route("api/euskalia/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ISocialAuthService _socialAuthService;
        private readonly IJwtService _jwtService;
        private readonly IEncryptionService _encryptionService;

        public AuthController(
            AppDbContext context,
            ISocialAuthService socialAuthService,
            IJwtService jwtService,
            IEncryptionService encryptionService)
        {
            _context = context;
            _socialAuthService = socialAuthService;
            _jwtService = jwtService;
            _encryptionService = encryptionService;
        }

        [HttpPost("social-login")]
        public async Task<IActionResult> SocialLogin([FromBody] SocialLoginDto socialLoginDto)
        {
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
                return Unauthorized(new { message = "Invalid social token" });
            }

            // Find user by email (encrypted in DB)
            var allUsers = await _context.Users.ToListAsync();
            var user = allUsers.FirstOrDefault(u => _encryptionService.Decrypt(u.Email) == socialUser.Email);

            if (user == null)
            {
                // Create unique nickname (Name-XXXX)
                var random = new Random();
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                string generatedNickname;
                bool isUnique = false;
                
                // Clean the base nickname (remove spaces, etc). If empty, use "User"
                string baseNickname = string.IsNullOrWhiteSpace(socialUser.Nickname) ? "User" : socialUser.Nickname.Replace(" ", "");
                
                do {
                    var suffix = new string(Enumerable.Repeat(chars, 4).Select(s => s[random.Next(s.Length)]).ToArray());
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
            }
            else if (!user.IsActive)
            {
                return Unauthorized(new { message = "Account deactivated" });
            }

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
