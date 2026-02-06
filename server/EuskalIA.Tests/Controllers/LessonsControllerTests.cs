using System.Collections.Generic;
using System.Threading.Tasks;
using EuskalIA.Server.Controllers;
using EuskalIA.Server.Models;
using EuskalIA.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace EuskalIA.Tests.Controllers
{
    public class LessonsControllerTests : TestBase
    {
        [Fact]
        public async Task GetLessons_SeedsData_WhenEmpty()
        {
            // Arrange
            var context = GetDatabaseContext();
            var mockAiService = new Mock<IAIService>();
            var mockEncrypt = new Mock<IEncryptionService>();
            mockEncrypt.Setup(e => e.Encrypt(It.IsAny<string>())).Returns((string s) => s);
            mockEncrypt.Setup(e => e.Decrypt(It.IsAny<string>())).Returns((string s) => s);

            mockAiService.Setup(x => x.GenerateExercisesAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new List<Exercise> { new Exercise { Question = "Test?" } });
            
            var controller = new LessonsController(context, mockAiService.Object, mockEncrypt.Object);

            // Act
            var result = await controller.GetLessons();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Lesson>>>(result);
            var lessons = Assert.IsAssignableFrom<IEnumerable<Lesson>>(actionResult.Value);
            Assert.NotEmpty(lessons);
        }

        [Fact]
        public async Task GetLesson_ReturnsNotFound_WhenInvalidId()
        {
            // Arrange
            var context = GetDatabaseContext();
            var mockAiService = new Mock<IAIService>();
            var mockEncrypt = new Mock<IEncryptionService>();
            var controller = new LessonsController(context, mockAiService.Object, mockEncrypt.Object);

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
            var mockEncrypt = new Mock<IEncryptionService>();
            mockAiService.Setup(x => x.GenerateExercisesAsync(It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(new List<Exercise> { new Exercise { Question = "New?" } });
            
            var controller = new LessonsController(context, mockAiService.Object, mockEncrypt.Object);

            // Act
            var result = await controller.GenerateLesson("Cultura");

            // Assert
            var actionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var lesson = Assert.IsType<Lesson>(actionResult.Value);
            Assert.Equal("Cultura", lesson.Title);
        }

        [Fact]
        public async Task GetLessons_ReturnsExistingData_WhenNotEmpty()
        {
            // Arrange
            var context = GetDatabaseContext();
            context.Lessons.Add(new Lesson { Title = "Existente", Topic = "Test" });
            await context.SaveChangesAsync();
            
            var mockAiService = new Mock<IAIService>();
            var mockEncrypt = new Mock<IEncryptionService>();
            mockEncrypt.Setup(e => e.Encrypt(It.IsAny<string>())).Returns((string s) => s);
            mockEncrypt.Setup(e => e.Decrypt(It.IsAny<string>())).Returns((string s) => s);
            
            var controller = new LessonsController(context, mockAiService.Object, mockEncrypt.Object);

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
            var mockEncrypt = new Mock<IEncryptionService>();
            var controller = new LessonsController(context, mockAiService.Object, mockEncrypt.Object);

            // Act
            var result = await controller.GetLesson(1);

            // Assert
            var foundLesson = Assert.IsType<Lesson>(result.Value);
            Assert.Equal(1, foundLesson.Id);
        }
    }
}
