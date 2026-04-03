using EuskalIA.Server.Controllers;
using EuskalIA.Server.Data;
using EuskalIA.Server.DTOs.Exercises;
using EuskalIA.Server.Models;
using EuskalIA.Server.Services.AI;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Microsoft.Extensions.Logging;

namespace EuskalIA.Tests.Controllers
{
    public class AigcExercisesControllerTests : TestBase
    {
        private Mock<IAIService> _mockAiService = new Mock<IAIService>();
        private Mock<IKnowledgeService> _mockKnowledgeService = new Mock<IKnowledgeService>();

        private AigcExercisesController CreateController(AppDbContext context)
        {
            var mockLogger = new Mock<ILogger<AigcExercisesController>>();
            return new AigcExercisesController(context, _mockAiService.Object, _mockKnowledgeService.Object, mockLogger.Object);
        }

        [Fact]
        public async Task GetExercisesByLevel_ReturnsOnlyMatchingLevel()
        {
            // Arrange
            using var context = GetDatabaseContext();
            
            context.AigcExercises.AddRange(
                new AigcExercise { LevelId = "A1", Status = "APPROVED", ExerciseCode = "1", TemplateType= "Type1", JsonSchema="{}" },
                new AigcExercise { LevelId = "A1", Status = "APPROVED", ExerciseCode = "2", TemplateType= "Type2", JsonSchema="{}" },
                new AigcExercise { LevelId = "B2", Status = "APPROVED", ExerciseCode = "3", TemplateType= "Type3", JsonSchema="{}" }
            );
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            // Act
            var result = await controller.GetExercises(levelId: "A1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var exercises = Assert.IsAssignableFrom<IEnumerable<AigcExerciseResponseDto>>(okResult.Value);
            Assert.Equal(2, exercises.Count());
            Assert.All(exercises, e => Assert.Equal("A1", e.LevelId));
        }

        [Fact]
        public async Task GetExercises_NeverReturnsRejected()
        {
            // Arrange
            using var context = GetDatabaseContext();
            
            context.AigcExercises.AddRange(
                new AigcExercise { LevelId = "A1", Status = "APPROVED", ExerciseCode = "1", TemplateType= "Type1", JsonSchema="{}" },
                new AigcExercise { LevelId = "A1", Status = "APPROVED", ExerciseCode = "2", TemplateType= "Type2", JsonSchema="{}" },
                new AigcExercise { LevelId = "A1", Status = "REJECTED", ExerciseCode = "3", TemplateType= "Type3", JsonSchema="{}" }
            );
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            // Act
            var result = await controller.GetExercises(levelId: "A1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var exercises = Assert.IsAssignableFrom<IEnumerable<AigcExerciseResponseDto>>(okResult.Value);
            
            Assert.Equal(2, exercises.Count());
            Assert.DoesNotContain(exercises, e => e.Status == "REJECTED");
        }

        [Fact]
        public async Task GetExerciseById_NotFound_Returns404()
        {
            // Arrange
            using var context = GetDatabaseContext();
            var controller = CreateController(context);

            // Act
            var result = await controller.GetExercise(Guid.NewGuid());

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateExercise_CreatesAndReturnsDto()
        {
            // Arrange
            using var context = GetDatabaseContext();
            var controller = CreateController(context);
            var dto = new AigcExerciseCreateDto
            {
                ExerciseCode = "ex-test-01",
                TemplateType = "block_builder",
                LevelId = "A1",
                Topics = "Grammar",
                Difficulty = 2,
                JsonSchema = "{\"test\": true}"
            };

            // Act
            var result = await controller.CreateExercise(dto);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var createdDto = Assert.IsType<AigcExerciseResponseDto>(createdAtActionResult.Value);
            
            Assert.Equal(dto.ExerciseCode, createdDto.ExerciseCode);
            Assert.Equal("BETA", createdDto.Status); // Defaults to BETA
            
            // Verify DB State
            Assert.Equal(1, context.AigcExercises.Count());
            var dbEntity = context.AigcExercises.First();
            Assert.Equal("BETA", dbEntity.Status);
        }
        [Fact]
        public async Task GetSessionExercises_WhenEmpty_TriggersSyncGeneration()
        {
            // Arrange
            using var context = GetDatabaseContext();
            var controller = CreateController(context);
            var levelId = "B1";

            _mockKnowledgeService.Setup(k => k.GetNextContextAsync(levelId))
                .ReturnsAsync(("context", "book", 1));
            
            _mockAiService.Setup(a => a.GenerateAigcExercisesAsync(levelId, "context", It.IsAny<int>()))
                .ReturnsAsync(new List<AigcExercise> { 
                    new AigcExercise { ExerciseCode = "new1", LevelId = levelId, TemplateType = "T1", JsonSchema = "{}" } 
                });

            // Act
            var result = await controller.GetSessionExercises(levelId, 1);

            // Assert
            _mockKnowledgeService.Verify(k => k.GetNextContextAsync(levelId), Times.AtLeastOnce());
            _mockAiService.Verify(a => a.GenerateAigcExercisesAsync(levelId, "context", It.IsAny<int>()), Times.AtLeastOnce());
        }
    }
}
