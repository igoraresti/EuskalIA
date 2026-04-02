using EuskalIA.Server.Models;
using EuskalIA.Server.Services.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EuskalIA.Tests.Services
{
    public class SocialAuthServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<HttpMessageHandler> _mockHandler;
        private readonly HttpClient _httpClient;
        private readonly Mock<ILogger<SocialAuthService>> _mockLogger;

        public SocialAuthServiceTests()
        {
            _mockConfig = new Mock<IConfiguration>();
            _mockHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHandler.Object);
            _mockLogger = new Mock<ILogger<SocialAuthService>>();
        }

        [Fact]
        public async Task ValidateGoogleTokenAsync_WithAccessToken_ReturnsUser()
        {
            // Arrange
            var token = "ya29.fake-token";
            var googleUser = new { email = "test@example.com", name = "Test User", email_verified = true };
            
            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("googleapis.com")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(googleUser))
                });

            var service = new SocialAuthService(_mockConfig.Object, _httpClient, _mockLogger.Object);

            // Act
            var result = await service.ValidateGoogleTokenAsync(token);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("test@example.com", result!.Email);
            Assert.True(result.IsVerified);
        }

        [Fact]
        public async Task ValidateFacebookTokenAsync_ReturnsUser_OnSuccess()
        {
            // Arrange
            var token = "fb-token";
            _mockConfig.Setup(c => c["Authentication:Facebook:AppId"]).Returns("app-id");
            _mockConfig.Setup(c => c["Authentication:Facebook:AppSecret"]).Returns("app-secret");

            // Mock Debug Token Response
            var debugResponse = new { data = new { is_valid = true } };
            // Mock User Info Response
            var userInfoResponse = new { id = "123", name = "FB User", email = "fb@example.com" };

            _mockHandler.Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(debugResponse))
                })
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(userInfoResponse))
                });

            var service = new SocialAuthService(_mockConfig.Object, _httpClient, _mockLogger.Object);

            // Act
            var result = await service.ValidateFacebookTokenAsync(token);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("fb@example.com", result!.Email);
            Assert.Equal("FB User", result.Nickname);
        }

        [Fact]
        public async Task ValidateFacebookTokenAsync_ReturnsNull_WhenTokenInvalid()
        {
            // Arrange
            var token = "bad-token";
            _mockConfig.Setup(c => c["Authentication:Facebook:AppId"]).Returns("app-id");
            _mockConfig.Setup(c => c["Authentication:Facebook:AppSecret"]).Returns("app-secret");

            var debugResponse = new { data = new { is_valid = false } };

            _mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(debugResponse))
                });

            var service = new SocialAuthService(_mockConfig.Object, _httpClient, _mockLogger.Object);

            // Act
            var result = await service.ValidateFacebookTokenAsync(token);

            // Assert
            Assert.Null(result);
        }
    }
}
