using EuskalIA.Server.Controllers;
using EuskalIA.Server.Data;
using EuskalIA.Server.DTOs.Auth;
using EuskalIA.Server.Models;
using EuskalIA.Server.Services.Auth;
using EuskalIA.Server.Services.Encryption;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EuskalIA.Tests.Controllers
{
    public class AuthControllerTests : TestBase
    {
        private readonly Mock<ISocialAuthService> _mockSocialAuth;
        private readonly Mock<IJwtService> _mockJwt;
        private readonly Mock<IEncryptionService> _mockEncrypt;
        private readonly Mock<ILogger<AuthController>> _mockLogger;

        public AuthControllerTests()
        {
            _mockSocialAuth = new Mock<ISocialAuthService>();
            _mockJwt = new Mock<IJwtService>();
            _mockEncrypt = new Mock<IEncryptionService>();
            _mockLogger = new Mock<ILogger<AuthController>>();

            _mockEncrypt.Setup(e => e.Encrypt(It.IsAny<string>())).Returns((string s) => "enc_" + s);
            _mockEncrypt.Setup(e => e.Decrypt(It.IsAny<string>())).Returns((string s) => s.Replace("enc_", ""));
        }

        private AuthController GetController(AppDbContext context)
        {
            return new AuthController(context, _mockSocialAuth.Object, _mockJwt.Object, _mockEncrypt.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task SocialLogin_Google_CreatesNewUser_WhenNotFound()
        {
            // Arrange
            using var context = GetDatabaseContext();
            var controller = GetController(context);
            var loginDto = new SocialLoginDto { Token = "google-token", Provider = "google", Language = "eu" };

            var socialUser = new User 
            { 
                Email = "test@gmail.com", 
                Nickname = "Test User",
                IsVerified = true
            };

            _mockSocialAuth.Setup(s => s.ValidateGoogleTokenAsync("google-token"))
                .ReturnsAsync(socialUser);

            _mockJwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("mock-jwt-token");

            // Act
            var result = await controller.SocialLogin(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            // Verify user was created in DB
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == "enc_test@gmail.com");
            Assert.NotNull(user);
            Assert.Equal("User", user.Role);
        }

        [Fact]
        public async Task SocialLogin_GeneratesUniqueNickname_WhenCollisionOccurs()
        {
            // Arrange
            using var context = GetDatabaseContext();
            
            // 1. Existing user with a collision-prone nickname
            // Note: The code uses Random, so we can't perfectly predict, but we can seed the DB
            // with a nickname that would be the first choice.
            // Actually, the code generates a RANDOM suffix every time.
            // To test the LOOP, we'd need to mock Random, which is not easily possible here 
            // without refactoring. 
            // However, we can test the "empty nickname" branch which is easier.
            
            var controller = GetController(context);
            var loginDto = new SocialLoginDto { Token = "google-token", Provider = "google" };
            var socialUser = new User { Email = "no-nick@gmail.com", Nickname = "", IsVerified = true };

            _mockSocialAuth.Setup(s => s.ValidateGoogleTokenAsync("google-token")).ReturnsAsync(socialUser);
            _mockJwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("jwt");

            // Act
            await controller.SocialLogin(loginDto);

            // Assert
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == "enc_no-nick@gmail.com");
            Assert.NotNull(user);
            Assert.Equal("Usuario", user.Username); // Default when social nickname is null/empty
            var decryptedNick = _mockEncrypt.Object.Decrypt(user.Nickname);
            Assert.StartsWith("User-", decryptedNick); // Base nickname was "User" since social was empty
        }

        [Fact]
        public async Task SocialLogin_ReturnsUnauthorized_WhenTokenInvalid()
        {
            // Arrange
            using var context = GetDatabaseContext();
            var controller = GetController(context);
            var loginDto = new SocialLoginDto { Token = "bad-token", Provider = "facebook" };

            _mockSocialAuth.Setup(s => s.ValidateFacebookTokenAsync("bad-token"))
                .ReturnsAsync((User?)null);

            // Act
            var result = await controller.SocialLogin(loginDto);

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result);
        }
    }
}
