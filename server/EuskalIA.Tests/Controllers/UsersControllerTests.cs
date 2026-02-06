using System;
using System.Threading.Tasks;
using EuskalIA.Server.Controllers;
using EuskalIA.Server.Models;
using EuskalIA.Server.DTOs;
using EuskalIA.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EuskalIA.Tests.Controllers
{
    public class UsersControllerTests : TestBase
    {
        [Fact]
        public async Task GetProgress_CreatesUserAndProgress_WhenNew()
        {
            // Arrange
            var context = GetDatabaseContext();
            var mockEncrypt = new Mock<IEncryptionService>();
            mockEncrypt.Setup(e => e.Encrypt(It.IsAny<string>())).Returns((string s) => s);
            mockEncrypt.Setup(e => e.Decrypt(It.IsAny<string>())).Returns((string s) => s);
            
            var controller = new UsersController(context, mockEncrypt.Object);

            // Act
            var result = await controller.GetProgress(1);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Progress>>(result);
            var progress = Assert.IsType<Progress>(actionResult.Value);
            Assert.Equal(1, progress.UserId);
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
            
            var mockEncrypt = new Mock<IEncryptionService>();
            var controller = new UsersController(context, mockEncrypt.Object);

            // Act
            var result = await controller.AddXP(1, new XPUpdateDto { XP = 1500, LessonTitle = "Test" });

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedProgress = Assert.IsType<Progress>(okResult.Value);
            Assert.Equal(1500, updatedProgress.XP);
            Assert.Equal(2, updatedProgress.Level);
        }

        [Fact]
        public async Task AddXP_ReturnsNotFound_WhenNoProgress()
        {
            // Arrange
            var context = GetDatabaseContext();
            var mockEncrypt = new Mock<IEncryptionService>();
            mockEncrypt.Setup(e => e.Encrypt(It.IsAny<string>())).Returns((string s) => s);
            mockEncrypt.Setup(e => e.Decrypt(It.IsAny<string>())).Returns((string s) => s);
            var controller = new UsersController(context, mockEncrypt.Object);

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
            
            var mockEncrypt = new Mock<IEncryptionService>();
            mockEncrypt.Setup(e => e.Decrypt("enc_email")).Returns("real_email");
            mockEncrypt.Setup(e => e.Decrypt("enc_nick")).Returns("real_nick");
            
            var controller = new UsersController(context, mockEncrypt.Object);

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
            
            var mockEncrypt = new Mock<IEncryptionService>();
            mockEncrypt.Setup(e => e.Encrypt(It.IsAny<string>())).Returns((string s) => "enc_" + s);
            
            var controller = new UsersController(context, mockEncrypt.Object);

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
            
            var mockEncrypt = new Mock<IEncryptionService>();
            mockEncrypt.Setup(e => e.Encrypt(It.IsAny<string>())).Returns((string s) => s);
            mockEncrypt.Setup(e => e.Decrypt(It.IsAny<string>())).Returns((string s) => s);
            var controller = new UsersController(context, mockEncrypt.Object);

            // Act
            var result = await controller.DeleteAccount(1, "123456");

            // Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.Null(await context.Users.FindAsync(1));
        }

        [Fact]
        public async Task GetProgress_ReturnsExisting_WhenUserExists()
        {
            // Arrange
            var context = GetDatabaseContext();
            var user = new User { Id = 1, Username = "Existing" };
            context.Users.Add(user);
            context.Progresses.Add(new Progress { UserId = 1, XP = 500 });
            await context.SaveChangesAsync();
            
            var mockEncrypt = new Mock<IEncryptionService>();
            mockEncrypt.Setup(e => e.Encrypt(It.IsAny<string>())).Returns((string s) => s);
            mockEncrypt.Setup(e => e.Decrypt(It.IsAny<string>())).Returns((string s) => s);
            var controller = new UsersController(context, mockEncrypt.Object);

            // Act
            var result = await controller.GetProgress(1);

            // Assert
            var progress = Assert.IsType<Progress>(result.Value);
            Assert.Equal(500, progress.XP);
        }
    }
}
