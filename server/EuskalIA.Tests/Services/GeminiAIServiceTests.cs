using System.Net;
using System.Text.Json;
using EuskalIA.Server.Models;
using EuskalIA.Server.Services.AI;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace EuskalIA.Tests.Services
{
    public class GeminiAIServiceTests
    {
        private readonly Mock<IOptions<GeminiSettings>> _mockOptions;
        private readonly Mock<ILogger<GeminiAIService>> _mockLogger;
        private readonly Mock<IServiceScopeFactory> _mockScopeFactory;
        private readonly GeminiSettings _settings;

        public GeminiAIServiceTests()
        {
            _mockOptions = new Mock<IOptions<GeminiSettings>>();
            _mockLogger = new Mock<ILogger<GeminiAIService>>();
            _mockScopeFactory = new Mock<IServiceScopeFactory>();
            
            _settings = new GeminiSettings { ApiKey = "test-key", Model = "gemini-1.5-flash" };
            _mockOptions.Setup(o => o.Value).Returns(_settings);

            // Mock Scope for logging
            var mockScope = new Mock<IServiceScope>();
            _mockScopeFactory.Setup(s => s.CreateScope()).Returns(mockScope.Object);
            mockScope.Setup(s => s.ServiceProvider).Returns(new Mock<IServiceProvider>().Object);
        }

        [Fact]
        public async Task GenerateAigcExercisesAsync_ReturnsParsedExercises()
        {
            // Arrange
            var mockResponseJson = new
            {
                candidates = new[]
                {
                    new
                    {
                        content = new
                        {
                            parts = new[]
                            {
                                new
                                {
                                    text = "[{\"exerciseCode\": \"test1\", \"templateType\": \"multiple_choice\", \"levelId\": \"A1\", \"jsonSchema\": \"{}\"}]"
                                }
                            }
                        }
                    }
                }
            };

            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(mockResponseJson))
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var service = new GeminiAIService(httpClient, _mockOptions.Object, _mockLogger.Object, _mockScopeFactory.Object);

            // Act
            var result = await service.GenerateAigcExercisesAsync("A1", "Context text", 1);

            // Assert
            result.Should().HaveCount(1);
            result[0].ExerciseCode.Should().Be("test1");
            result[0].LevelId.Should().Be("A1");
        }

        [Fact]
        public async Task GenerateAigcExercisesAsync_WhenApiFails_ReturnsEmptyList()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent("Error body")
                });

            var httpClient = new HttpClient(handlerMock.Object);
            var service = new GeminiAIService(httpClient, _mockOptions.Object, _mockLogger.Object, _mockScopeFactory.Object);

            // Act
            var result = await service.GenerateAigcExercisesAsync("A1", "Context text", 1);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GenerateAigcExercisesAsync_WhenApiKeyMissing_ReturnsEmptyList()
        {
            // Arrange
            _settings.ApiKey = "";
            var httpClient = new HttpClient(new Mock<HttpMessageHandler>().Object);
            var service = new GeminiAIService(httpClient, _mockOptions.Object, _mockLogger.Object, _mockScopeFactory.Object);

            // Act
            var result = await service.GenerateAigcExercisesAsync("A1", "Context text", 1);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GenerateAigcExercisesAsync_WhenJsonIsInvalid_ReturnsEmptyList()
        {
            // Arrange
            var mockResponseJson = new { candidates = new[] { new { content = new { parts = new[] { new { text = "invalid-json" } } } } } };
            
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(JsonSerializer.Serialize(mockResponseJson)) });

            var httpClient = new HttpClient(handlerMock.Object);
            var service = new GeminiAIService(httpClient, _mockOptions.Object, _mockLogger.Object, _mockScopeFactory.Object);

            // Act
            var result = await service.GenerateAigcExercisesAsync("A1", "Context text", 1);

            // Assert
            result.Should().BeEmpty();
        }
    }
}
