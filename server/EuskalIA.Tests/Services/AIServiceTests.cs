using EuskalIA.Server.Services;
using FluentAssertions;
using Xunit;

namespace EuskalIA.Tests.Services
{
    public class AIServiceTests
    {
        [Fact]
        public async Task GenerateExercisesAsync_WithSaludosTopic_ReturnsSpecificExercises()
        {
            // Arrange
            var service = new MockAIService();
            var topic = "saludos";
            var count = 3;

            // Act
            var result = await service.GenerateExercisesAsync(topic, count);

            // Assert
            result.Should().HaveCount(2);
            result[0].Question.Should().Contain("Hola");
            result[0].CorrectAnswer.Should().Be("Kaixo");
            result[1].Question.Should().Contain("Adiós");
            result[1].CorrectAnswer.Should().Be("Agur");
        }

        [Fact]
        public async Task GenerateExercisesAsync_WithGenericTopic_ReturnsRequestedCount()
        {
            // Arrange
            var service = new MockAIService();
            var topic = "comida";
            var count = 5;

            // Act
            var result = await service.GenerateExercisesAsync(topic, count);

            // Assert
            result.Should().HaveCount(count);
            result.Should().AllSatisfy(ex => {
                ex.Question.Should().Contain(topic);
                ex.Type.Should().Be("MultipleChoice");
                ex.OptionsJson.Should().NotBeNullOrEmpty();
            });
        }
    }
}
