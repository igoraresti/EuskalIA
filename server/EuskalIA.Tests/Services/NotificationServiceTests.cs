using EuskalIA.Server.Services.Notifications;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EuskalIA.Tests.Services
{
    public class NotificationServiceTests
    {
        private readonly Mock<HttpMessageHandler> _mockHandler;
        private readonly HttpClient _httpClient;
        private readonly Mock<ILogger<NotificationService>> _mockLogger;

        public NotificationServiceTests()
        {
            _mockHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHandler.Object);
            _mockLogger = new Mock<ILogger<NotificationService>>();
        }

        [Fact]
        public async Task SendPushNotificationAsync_SendsRequest_WhenTokenValid()
        {
            // Arrange
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            var service = new NotificationService(_httpClient, _mockLogger.Object);

            // Act
            await service.SendPushNotificationAsync("token", "Title", "Body");

            // Assert
            _mockHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task SendPushNotificationAsync_DoesNotSend_WhenTokenEmpty()
        {
            // Arrange
            var service = new NotificationService(_httpClient, _mockLogger.Object);

            // Act
            await service.SendPushNotificationAsync("", "Title", "Body");

            // Assert
            _mockHandler.Protected().Verify(
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task SendPushNotificationsAsync_ChunksLargeBatches()
        {
            // Arrange
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK
                });

            var service = new NotificationService(_httpClient, _mockLogger.Object);
            var tokens = new List<string>();
            for (int i = 0; i < 150; i++) tokens.Add($"token-{i}");

            // Act
            await service.SendPushNotificationsAsync(tokens, "Batch", "Message");

            // Assert: Should be 2 calls (100 + 50)
            _mockHandler.Protected().Verify(
                "SendAsync",
                Times.Exactly(2),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task SendPushNotificationsAsync_LogsError_WhenApiFails()
        {
            // Arrange
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Error")
                });

            var service = new NotificationService(_httpClient, _mockLogger.Object);

            // Act
            await service.SendPushNotificationAsync("token", "T", "B");

            // Assert: Logger should have logged error (one call to LogError)
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to send")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
