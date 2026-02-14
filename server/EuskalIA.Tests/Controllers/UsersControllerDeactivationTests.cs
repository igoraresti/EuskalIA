using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EuskalIA.Server.Controllers;
using EuskalIA.Server.Data;
using EuskalIA.Server.Models;
using EuskalIA.Server.Services;
using System;
using System.Threading.Tasks;

namespace EuskalIA.Tests.Controllers
{
    public class UsersControllerDeactivationTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly Mock<IEncryptionService> _mockEncryptionService;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly UsersController _controller;

        public UsersControllerDeactivationTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB per test
                .Options;

            _context = new AppDbContext(options);
            _context.Database.EnsureCreated();

            _mockEncryptionService = new Mock<IEncryptionService>();
            _mockEncryptionService.Setup(s => s.Encrypt(It.IsAny<string>())).Returns((string s) => "ENC_" + s);
            _mockEncryptionService.Setup(s => s.Decrypt(It.IsAny<string>())).Returns((string s) => s.Replace("ENC_", ""));

            _mockEmailService = new Mock<IEmailService>();

            _controller = new UsersController(_context, _mockEncryptionService.Object, _mockEmailService.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public async Task RequestDeactivation_ShouldGenerateToken_AndSendEmail()
        {
            // Arrange
            var user = new User { Username = "TestUser", Email = "ENC_test@example.com", IsActive = true };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.RequestDeactivation(user.Id);

            // Assert
            result.Should().BeOfType<OkObjectResult>();

            var updatedUser = await _context.Users.FindAsync(user.Id);
            updatedUser.Should().NotBeNull();
            updatedUser!.DeactivationToken.Should().NotBeNullOrEmpty();
            updatedUser.DeactivationTokenExpiration.Should().BeAfter(DateTime.UtcNow);

            _mockEmailService.Verify(s => s.SendDeactivationEmailAsync("test@example.com", "TestUser", updatedUser.DeactivationToken!), Times.Once);
        }

        [Fact]
        public async Task ConfirmDeactivation_WithValidToken_ShouldDeactivateUser()
        {
            // Arrange
            var token = Guid.NewGuid().ToString();
            var user = new User 
            { 
                Username = "TestUser", 
                Email = "ENC_test@example.com", 
                IsActive = true,
                DeactivationToken = token,
                DeactivationTokenExpiration = DateTime.UtcNow.AddHours(1)
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.ConfirmDeactivation(token);

            // Assert
            result.Should().BeOfType<RedirectResult>();
            var redirectResult = result as RedirectResult;
            redirectResult!.Url.Should().Contain("deactivated=true");

            var updatedUser = await _context.Users.FindAsync(user.Id);
            updatedUser!.IsActive.Should().BeFalse();
            updatedUser.DeactivationToken.Should().BeNull();
            updatedUser.DeactivationTokenExpiration.Should().BeNull();
        }

        [Fact]
        public async Task ConfirmDeactivation_WithInvalidToken_ShouldReturnBadRequest()
        {
             // Act
            var result = await _controller.ConfirmDeactivation("invalid-token");

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task ConfirmDeactivation_WithExpiredToken_ShouldReturnBadRequest()
        {
            // Arrange
            var token = Guid.NewGuid().ToString();
            var user = new User 
            { 
                Username = "TestUser", 
                IsActive = true,
                DeactivationToken = token,
                DeactivationTokenExpiration = DateTime.UtcNow.AddHours(-1) // Expired
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.ConfirmDeactivation(token);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            
            var updatedUser = await _context.Users.FindAsync(user.Id);
            updatedUser!.IsActive.Should().BeTrue(); // Should likely remain active or at least not be deactivated successfully
        }

        [Fact]
        public async Task Login_WithDeactivatedUser_ShouldReturnUnauthorized()
        {
             // Arrange
            var user = new User 
            { 
                Username = "InactiveUser", 
                Password = "ENC_Password123",
                IsActive = false,
                IsVerified = true
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var loginDto = new EuskalIA.Server.DTOs.LoginDto
            {
                Username = "InactiveUser",
                Password = "Password123"
            };

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            result.Result.Should().BeOfType<UnauthorizedObjectResult>();
        }
    }
}
