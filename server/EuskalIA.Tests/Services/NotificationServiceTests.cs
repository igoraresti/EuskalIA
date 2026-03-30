using Moq;
using Moq.Protected;
using System.Net;
using Microsoft.Extensions.Logging;
using EuskalIA.Server.Services.Notifications;

namespace EuskalIA.Tests.Services
{
    public class NotificationServiceTests
    {
        private readonly Mock<HttpMessageHandler> _msgHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly Mock<ILogger<NotificationService>> _loggerMock;
        private readonly NotificationService _service;

        public NotificationServiceTests()
        {
            _msgHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_msgHandlerMock.Object);
            _loggerMock = new Mock<ILogger<NotificationService>>();
            _service = new NotificationService(_httpClient, _loggerMock.Object);
        }

        [Fact]
        public async Task SendPushNotificationAsync_SendsCorrectRequest()
        {
            // Arrange
            _msgHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{ \"data\": [] }")
                });

            var token = "ExponentPushToken[123]";
            var title = "Test Title";
            var body = "Test Body";

            // Act
            await _service.SendPushNotificationAsync(token, title, body);

            // Assert
            _msgHandlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.ToString() == "https://exp.host/--/api/v2/push/send"),
                ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}
