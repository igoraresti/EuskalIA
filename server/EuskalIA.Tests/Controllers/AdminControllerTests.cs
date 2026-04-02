using EuskalIA.Server.Controllers;
using EuskalIA.Server.Data;
using EuskalIA.Server.DTOs;
using EuskalIA.Server.Models;
using EuskalIA.Server.Services.Encryption;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EuskalIA.Tests.Controllers
{
    public class AdminControllerTests : TestBase
    {
        private readonly Mock<IEncryptionService> _mockEncrypt;
        private readonly Mock<ILogger<AdminController>> _mockLogger;

        public AdminControllerTests()
        {
            _mockEncrypt = new Mock<IEncryptionService>();
            _mockLogger = new Mock<ILogger<AdminController>>();
            
            _mockEncrypt.Setup(e => e.Decrypt(It.IsAny<string>())).Returns((string s) => s);
            _mockEncrypt.Setup(e => e.Encrypt(It.IsAny<string>())).Returns((string s) => s);
        }

        private AdminController GetController(AppDbContext context)
        {
            return new AdminController(context, _mockEncrypt.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetUsers_ReturnsPaginatedUsers_WithFilters()
        {
            // Arrange
            using var context = GetDatabaseContext();
            var controller = GetController(context);
            
            var user1 = new User { Username = "active_user", IsActive = true, JoinedAt = DateTime.UtcNow.AddDays(-5) };
            var user2 = new User { Username = "inactive_user", IsActive = false, JoinedAt = DateTime.UtcNow.AddDays(-1) };
            context.Users.AddRange(user1, user2);
            await context.SaveChangesAsync();

            // Act: Filter by active
            var filter = new AdminUserFilterDto { IsActive = true, Page = 1, PageSize = 10 };
            var result = await controller.GetUsers(filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsType<PaginatedList<AdminUserDto>>(okResult.Value);
            Assert.Equal(1, list.TotalCount);
            Assert.Equal("active_user", list.Items.First().Username);
        }

        [Fact]
        public async Task ToggleUserActive_ChangesStatus()
        {
            // Arrange
            using var context = GetDatabaseContext();
            var controller = GetController(context);
            
            var user = new User { Id = 1, Username = "test", IsActive = true };
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.ToggleUserActive(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedUser = await context.Users.FindAsync(1);
            Assert.False(updatedUser!.IsActive);
        }

        [Fact]
        public async Task DeleteExercise_RemovesExerciseAndAttempts()
        {
            // Arrange
            using var context = GetDatabaseContext();
            var controller = GetController(context);
            
            var exId = Guid.NewGuid();
            var exercise = new AigcExercise { Id = exId, ExerciseCode = "to-delete", LevelId = "A1", TemplateType = "T1", JsonSchema = "{}" };
            context.AigcExercises.Add(exercise);
            context.UserExerciseAttempts.Add(new UserExerciseAttempt { ExerciseId = exId, UserId = 1, IsCorrect = true });
            await context.SaveChangesAsync();

            // Act
            var result = await controller.DeleteExercise(exId);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.Null(await context.AigcExercises.FindAsync(exId));
            Assert.Empty(context.UserExerciseAttempts.Where(a => a.ExerciseId == exId));
        }

        [Fact]
        public async Task BulkUpdateStatus_UpdatesMultipleExercises()
        {
            // Arrange
            using var context = GetDatabaseContext();
            var controller = GetController(context);
            
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            context.AigcExercises.AddRange(
                new AigcExercise { Id = id1, ExerciseCode = "E1", Status = "BETA", LevelId = "A1", TemplateType = "T1", JsonSchema = "{}" },
                new AigcExercise { Id = id2, ExerciseCode = "E2", Status = "BETA", LevelId = "A1", TemplateType = "T1", JsonSchema = "{}" }
            );
            await context.SaveChangesAsync();

            // Act
            var req = new AdminController.BulkStatusRequest(new List<Guid> { id1, id2 }, "APPROVED");
            var result = await controller.BulkUpdateStatus(req);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            Assert.All(context.AigcExercises.ToList(), e => Assert.Equal("APPROVED", e.Status));
        }

        [Fact]
        public async Task ImportExercises_DetectsDuplicates_AndInsertsOnConfirm()
        {
            // Arrange
            using var context = GetDatabaseContext();
            var controller = GetController(context);
            
            // 1. Existing exercise in DB
            var existingJson = "{\"question\": {\"es\": \"¿cómo estás?\", \"en\": \"how are you?\"}}";
            context.AigcExercises.Add(new AigcExercise 
            { 
                ExerciseCode = "a1_grammar_0", 
                LevelId = "A1", 
                Topics = "grammar", 
                JsonSchema = existingJson,
                Status = "APPROVED",
                TemplateType = "T1"
            });
            await context.SaveChangesAsync();

            // 2. Import request with one duplicate and one new
            var req = new AdminController.ImportRequest(
                Exercises: new List<AdminController.ImportExerciseItem>
                {
                    // Duplicate (high similarity)
                    new AdminController.ImportExerciseItem("T1", "A1", "grammar", 1, existingJson),
                    // New
                    new AdminController.ImportExerciseItem("T1", "A1", "vocabulary", 1, "{\"question\": {\"es\": \"hola\", \"en\": \"hello\"}}")
                },
                Confirm: true,
                Threshold: 0.8
            );

            // Act
            var result = await controller.ImportExercises(req);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            // Verify results using reflection for anonymous type
            var value = okResult.Value!;
            var importedProp = value.GetType().GetProperty("imported");
            Assert.Equal(1, (int)importedProp!.GetValue(value)!);
            
            // Verify DB: one new exercise added
            Assert.Equal(2, context.AigcExercises.Count());
            Assert.Contains(context.AigcExercises.ToList(), e => e.Topics == "vocabulary");
        }

        [Fact]
        public async Task GetExercises_ReturnsAllExercises()
        {
            // Arrange
            using var context = GetDatabaseContext();
            context.AigcExercises.AddRange(new List<AigcExercise>
            {
                new AigcExercise { ExerciseCode = "E1", LevelId = "A1", TemplateType = "T1", JsonSchema = "{}" },
                new AigcExercise { ExerciseCode = "E2", LevelId = "A2", TemplateType = "T1", JsonSchema = "{}" }
            });
            await context.SaveChangesAsync();
            var controller = GetController(context);

            // Act
            var result = await controller.GetExercises(null, null, null, null, "exerciseCode", "asc", 1, 20);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value!;
            var totalProp = value.GetType().GetProperty("total");
            Assert.Equal(2, (int)totalProp!.GetValue(value)!);
        }

        [Fact]
        public async Task DeleteExercise_RemovesFromDb()
        {
            // Arrange
            using var context = GetDatabaseContext();
            var exerciseId = Guid.NewGuid();
            var ex = new AigcExercise { Id = exerciseId, ExerciseCode = "DELETE_ME", LevelId = "A1", TemplateType = "T1", JsonSchema = "{}" };
            context.AigcExercises.Add(ex);
            await context.SaveChangesAsync();
            var controller = GetController(context);

            // Act
            var result = await controller.DeleteExercise(exerciseId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.False(context.AigcExercises.Any(e => e.Id == exerciseId));
        }

        [Fact]
        public async Task GetStats_ReturnsCorrectCounts()
        {
            // Arrange
            using var context = GetDatabaseContext();
            context.Users.Add(new User { Username = "U1", Email = "E1", Nickname = "N1", Password = "P1" });
            context.AigcExercises.Add(new AigcExercise { ExerciseCode = "E1", LevelId = "A1", TemplateType = "T1", JsonSchema = "{}", Status = "APPROVED" });
            await context.SaveChangesAsync();
            var controller = GetController(context);

            // Act
            var result = await controller.GetStats();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = okResult.Value!;
            Assert.Equal(1, (int)value.GetType().GetProperty("TotalUsers")!.GetValue(value)!);
            Assert.Equal(1, (int)value.GetType().GetProperty("ActiveUsers")!.GetValue(value)!);
        }

        [Fact]
        public async Task ToggleUserActive_ChangesActiveStatus()
        {
            // Arrange
            using var context = GetDatabaseContext();
            var user = new User { Username = "U1", Email = "E1", Nickname = "N1", Password = "P1", IsActive = true };
            context.Users.Add(user);
            await context.SaveChangesAsync();
            var controller = GetController(context);

            // Act
            await controller.ToggleUserActive(user.Id);

            // Assert
            Assert.False(user.IsActive);

            // Act again
            await controller.ToggleUserActive(user.Id);

            // Assert
            Assert.True(user.IsActive);
        }
    }
}
