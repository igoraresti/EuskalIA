using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EuskalIA.Server.Controllers;
using EuskalIA.Server.Data;
using EuskalIA.Server.Models;
using EuskalIA.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace EuskalIA.Tests
{
    public class ControllerTests
    {
        private AppDbContext GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            var databaseContext = new AppDbContext(options);
            databaseContext.Database.EnsureCreated();
            return databaseContext;
        }

        [Fact]
        public async Task GetLessons_SeedsData_WhenEmpty()
        {
            // Arrange
            var context = GetDatabaseContext();
            var mockAiService = new Mock<IAIService>();
            mockAiService.Setup(x => x.GenerateExercisesAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new List<Exercise> { new Exercise { Question = "Test?" } });
            
            var controller = new LessonsController(context, mockAiService.Object);

            // Act
            var result = await controller.GetLessons();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Lesson>>>(result);
            var lessons = Assert.IsAssignableFrom<IEnumerable<Lesson>>(actionResult.Value);
            Assert.NotEmpty(lessons);
            Assert.Equal(4, lessons.Count()); // Saludos, Comida, Bar, Viajes
        }

        [Fact]
        public async Task GetLesson_ReturnsNotFound_WhenInvalidId()
        {
            // Arrange
            var context = GetDatabaseContext();
            var mockAiService = new Mock<IAIService>();
            var controller = new LessonsController(context, mockAiService.Object);

            // Act
            var result = await controller.GetLesson(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GenerateLesson_CreatesNewLesson()
        {
            // Arrange
            var context = GetDatabaseContext();
            var mockAiService = new Mock<IAIService>();
            mockAiService.Setup(x => x.GenerateExercisesAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new List<Exercise> { new Exercise { Question = "New?" } });
            
            var controller = new LessonsController(context, mockAiService.Object);

            // Act
            var result = await controller.GenerateLesson("Cultura");

            // Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var lesson = Assert.IsType<Lesson>(actionResult.Value);
            Assert.Equal("Cultura", lesson.Title);
            Assert.Single(lesson.Exercises);
        }

        [Fact]
        public async Task GetProgress_CreatesUserAndProgress_WhenNew()
        {
            // Arrange
            var context = GetDatabaseContext();
            var controller = new UsersController(context);

            // Act
            var result = await controller.GetProgress(1);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Progress>>(result);
            var progress = Assert.IsType<Progress>(actionResult.Value);
            Assert.Equal(1, progress.UserId);
            Assert.Equal(100, progress.Txanponak);
        }

        [Fact]
        public async Task AddXP_UpdatesProgress()
        {
            // Arrange
            var context = GetDatabaseContext();
            var progress = new Progress { UserId = 1, XP = 0, Level = 1 };
            context.Progresses.Add(progress);
            await context.SaveChangesAsync();
            
            var controller = new UsersController(context);

            // Act
            var result = await controller.AddXP(1, 1500);

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
            var controller = new UsersController(context);

            // Act
            var result = await controller.AddXP(999, 100);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task GetLessons_ReturnsExistingData_WhenNotEmpty()
        {
            // Arrange
            var context = GetDatabaseContext();
            context.Lessons.Add(new Lesson { Title = "Existente", Topic = "Test" });
            await context.SaveChangesAsync();
            
            var mockAiService = new Mock<IAIService>();
            var controller = new LessonsController(context, mockAiService.Object);

            // Act
            var result = await controller.GetLessons();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Lesson>>>(result);
            var lessons = Assert.IsAssignableFrom<IEnumerable<Lesson>>(actionResult.Value);
            Assert.Single(lessons);
            mockAiService.Verify(x => x.GenerateExercisesAsync(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetLesson_ReturnsLesson_WhenValidId()
        {
            // Arrange
            var context = GetDatabaseContext();
            var lesson = new Lesson { Id = 1, Title = "Test" };
            context.Lessons.Add(lesson);
            await context.SaveChangesAsync();
            
            var mockAiService = new Mock<IAIService>();
            var controller = new LessonsController(context, mockAiService.Object);

            // Act
            var result = await controller.GetLesson(1);

            // Assert
            var foundLesson = Assert.IsType<Lesson>(result.Value);
            Assert.Equal(1, foundLesson.Id);
        }

        [Fact]
        public async Task GetProgress_ReturnsExisting_WhenUserExists()
        {
            // Arrange
            var context = GetDatabaseContext();
            context.Users.Add(new User { Id = 1, Username = "Existing" });
            context.Progresses.Add(new Progress { UserId = 1, XP = 500 });
            await context.SaveChangesAsync();
            
            var controller = new UsersController(context);

            // Act
            var result = await controller.GetProgress(1);

            // Assert
            var progress = Assert.IsType<Progress>(result.Value);
            Assert.Equal(500, progress.XP);
        }
    }
}
