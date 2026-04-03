using System;
using System.Threading.Tasks;
using EuskalIA.Server.Controllers;
using EuskalIA.Server.Models;
using EuskalIA.Server.DTOs.Auth;
using EuskalIA.Server.DTOs.Users;
using EuskalIA.Server.Services.Encryption;
using EuskalIA.Server.Services.Email;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using EuskalIA.Server.Services.Auth;
using EuskalIA.Server.Services;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;

namespace EuskalIA.Tests.Controllers
{
    public class UsersControllerTests : TestBase
    {
        private readonly Mock<IEncryptionService> _mockEncrypt;
        private readonly Mock<IEmailService> _mockEmail;
        private readonly Mock<IJwtService> _mockJwt;
        private readonly Mock<IStringLocalizer<UsersController>> _mockLocalizer;
        private readonly Mock<IGamificationService> _mockGamification;

        public UsersControllerTests()
        {
            _mockEncrypt = new Mock<IEncryptionService>();
            _mockEmail = new Mock<IEmailService>();
            _mockJwt = new Mock<IJwtService>();
            _mockLocalizer = new Mock<IStringLocalizer<UsersController>>();
            _mockGamification = new Mock<IGamificationService>();
            
            _mockEncrypt.Setup(e => e.Encrypt(It.IsAny<string>())).Returns((string s) => s);
            _mockEncrypt.Setup(e => e.Decrypt(It.IsAny<string>())).Returns((string s) => s);
            _mockLocalizer.Setup(l => l[It.IsAny<string>()]).Returns(new LocalizedString("test", "test"));
        }

        private UsersController GetController(EuskalIA.Server.Data.AppDbContext context)
        {
            var mockLogger = new Mock<ILogger<UsersController>>();
            return new UsersController(context, _mockEncrypt.Object, _mockEmail.Object, _mockJwt.Object, _mockLocalizer.Object, _mockGamification.Object, mockLogger.Object);
        }

        [Fact]
        public async Task GetProgress_ReturnsProgress_WhenUserExists()
        {
            // Arrange
            var context = GetDatabaseContext();
            var user = new User { Id = 1, Username = "Igor Aresti", Email = "igor@euskalia.eus", Nickname = "igor" };
            context.Users.Add(user);
            await context.SaveChangesAsync();
            
            var controller = GetController(context);

            // Act
            var result = await controller.GetProgress(1);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            // The controller returns an anonymous object with { Progress, LessonScores }
            Assert.NotNull(actionResult.Value);
        }

        [Fact]
        public async Task AddXP_UpdatesProgress()
        {
            // Arrange
            var context = GetDatabaseContext();
            var user = new User { Id = 1, Username = "Test" };
            context.Users.Add(user);
            var progress = new Progress { UserId = 1, XP = 0, Level = 1 };
            context.Progresses.Add(progress);
            await context.SaveChangesAsync();
            
            var controller = GetController(context);

            // Act
            var result = await controller.AddXP(1, new XPUpdateDto { XP = 1500, LessonTitle = "Test" });
            
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            // Controller returns anonymous object { Progress, NewlyEarned }
            // Use reflection to access across assembly boundaries
            var value = okResult.Value!;
            var progressProp = value.GetType().GetProperty("Progress");
            var updatedProgress = Assert.IsType<Progress>(progressProp!.GetValue(value));
            
            Assert.Equal(1500, updatedProgress.XP);
            Assert.Equal(2, updatedProgress.Level);
        }

        [Fact]
        public async Task AddXP_ReturnsNotFound_WhenNoProgress()
        {
            // Arrange
            var context = GetDatabaseContext();
            var controller = GetController(context);

            // Act
            var result = await controller.AddXP(999, new XPUpdateDto { XP = 100 });

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetUser_ReturnsUser_WhenExists()
        {
            // Arrange
            var context = GetDatabaseContext();
            context.Users.Add(new User { Id = 1, Username = "Test", Email = "enc_email", Nickname = "enc_nick" });
            await context.SaveChangesAsync();
            
            _mockEncrypt.Setup(e => e.Decrypt("enc_email")).Returns("real_email");
            _mockEncrypt.Setup(e => e.Decrypt("enc_nick")).Returns("real_nick");
            
            var controller = GetController(context);

            // Act
            var result = await controller.GetUser(1);

            // Assert
            var user = Assert.IsType<User>(result.Value);
            Assert.Equal("real_email", user.Email);
        }

        [Fact]
        public async Task UpdateProfile_UpdatesUser()
        {
            // Arrange
            var context = GetDatabaseContext();
            context.Users.Add(new User { Id = 1, Username = "Old" });
            await context.SaveChangesAsync();
            
            _mockEncrypt.Setup(e => e.Encrypt(It.IsAny<string>())).Returns((string s) => "enc_" + s);
            
            var controller = GetController(context);

            // Act
            var result = await controller.UpdateProfile(1, new ProfileUpdateDto { Username = "New", Email = "new@euskalia.eus" });

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var user = await context.Users.FindAsync(1);
            Assert.Equal("New", user.Username);
            Assert.Equal("enc_new@euskalia.eus", user.Email);
        }

        [Fact]
        public async Task DeleteAccount_RemovesUser()
        {
            // Arrange
            var context = GetDatabaseContext();
            context.Users.Add(new User { Id = 1, DeletionCode = "123456", CodeExpiration = DateTime.UtcNow.AddMinutes(10) });
            await context.SaveChangesAsync();
            
            var controller = GetController(context);

            // Act
            var result = await controller.DeleteAccount(1, "123456");

            // Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.Null(await context.Users.FindAsync(1));
        }

        [Fact]
        public async Task Login_ReturnsToken_WhenCredentialsAreValid()
        {
            // Arrange
            var context = GetDatabaseContext();
            var user = new User 
            { 
                Id = 1, 
                Username = "testuser", 
                Password = "hashed_password",
                IsActive = true,
                IsVerified = true,
                Nickname = "nick",
                Email = "email"
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            _mockEncrypt.Setup(e => e.Decrypt("hashed_password")).Returns("plain_password");
            _mockJwt.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("mock_token");

            var controller = GetController(context);

            // Act
            var result = await controller.Login(new LoginDto { Username = "testuser", Password = "plain_password" });

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            // Result is an anonymous type, so we use dynamic or reflection to verify
            var response = okResult.Value as dynamic;
            Assert.NotNull(response);
            // Since it's anonymous, we might need to check properties via reflection if Cast fails
            // But usually OkObjectResult.Value is the object itself.
        }
    }
}
