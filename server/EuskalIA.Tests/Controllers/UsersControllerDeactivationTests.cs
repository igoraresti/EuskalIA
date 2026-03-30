using System;
using System.Threading.Tasks;
using EuskalIA.Server.Controllers;
using EuskalIA.Server.Models;
using EuskalIA.Server.DTOs;
using EuskalIA.Server.Services.Encryption;
using EuskalIA.Server.Services.Email;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using EuskalIA.Server.Services.Auth;
using EuskalIA.Server.Services;
using Moq;
using Xunit;

namespace EuskalIA.Tests.Controllers
{
    public class UsersControllerDeactivationTests : TestBase
    {
        private readonly Mock<IEncryptionService> _mockEncrypt;
        private readonly Mock<IEmailService> _mockEmail;
        private readonly Mock<IJwtService> _mockJwt;
        private readonly Mock<IStringLocalizer<UsersController>> _mockLocalizer;
        private readonly Mock<IGamificationService> _mockGamification;

        public UsersControllerDeactivationTests()
        {
            _mockEncrypt = new Mock<IEncryptionService>();
            _mockEmail = new Mock<IEmailService>();
            _mockJwt = new Mock<IJwtService>();
            _mockLocalizer = new Mock<IStringLocalizer<UsersController>>();
            _mockGamification = new Mock<IGamificationService>();

            _mockEncrypt.Setup(e => e.Decrypt(It.IsAny<string>())).Returns((string s) => s);
            _mockLocalizer.Setup(l => l[It.IsAny<string>()]).Returns(new LocalizedString("test", "test"));
        }

        private UsersController GetController(EuskalIA.Server.Data.AppDbContext context)
        {
            return new UsersController(context, _mockEncrypt.Object, _mockEmail.Object, _mockJwt.Object, _mockLocalizer.Object, _mockGamification.Object);
        }

        [Fact]
        public async Task RequestDeactivation_SetsTokenAndSendsEmail()
        {
            // Arrange
            var context = GetDatabaseContext();
            var user = new User { Id = 1, Username = "testuser", Email = "encrypted_email", IsActive = true };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var controller = GetController(context);

            // Act
            var result = await controller.RequestDeactivation(1);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var updatedUser = await context.Users.FindAsync(1);
            Assert.NotNull(updatedUser.DeactivationToken);
            Assert.NotNull(updatedUser.DeactivationTokenExpiration);
            
            _mockEmail.Verify(e => e.SendDeactivationEmailAsync("encrypted_email", "testuser", updatedUser.DeactivationToken), Times.Once);
        }

        [Fact]
        public async Task ConfirmDeactivation_DeactivatesUser_WhenTokenValid()
        {
            // Arrange
            var context = GetDatabaseContext();
            var token = "valid_token";
            var user = new User 
            { 
                Id = 1, 
                Username = "testuser", 
                IsActive = true, 
                DeactivationToken = token,
                DeactivationTokenExpiration = DateTime.UtcNow.AddHours(1)
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var controller = GetController(context);

            // Act
            var result = await controller.ConfirmDeactivation(token);

            // Assert
            var redirectResult = Assert.IsType<RedirectResult>(result);
            Assert.Contains("deactivated=true", redirectResult.Url);
            
            var deactivatedUser = await context.Users.FindAsync(1);
            Assert.False(deactivatedUser.IsActive);
            Assert.Null(deactivatedUser.DeactivationToken);
        }
    }
}
