using EuskalIA.Server.Services;
using EuskalIA.Server.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Moq;
using System.Globalization;
using Xunit;

namespace EuskalIA.Tests.Services
{
    public class EmailServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly Mock<IStringLocalizer<EmailService>> _mockLocalizer;

        public EmailServiceTests()
        {
            _mockConfig = new Mock<IConfiguration>();
            _mockLocalizer = new Mock<IStringLocalizer<EmailService>>();
        }

        [Fact]
        public async Task SendVerificationEmailAsync_SwitchesCultureAndRetrievesLocalizedStrings()
        {
            // Arrange
            var section = new Mock<IConfigurationSection>();
            section.Setup(s => s.Value).Returns("true");
            _mockConfig.Setup(c => c.GetSection("EmailSettings:UseMockService")).Returns(section.Object);

            var localizedString = new LocalizedString("VerifySubject", "Test Subject");
            _mockLocalizer.Setup(l => l["VerifySubject"]).Returns(localizedString);
            _mockLocalizer.Setup(l => l["VerifyGreeting"]).Returns(new LocalizedString("VerifyGreeting", "Hello {0}"));
            _mockLocalizer.Setup(l => l["VerifyThanks"]).Returns(new LocalizedString("VerifyThanks", "Thanks"));
            _mockLocalizer.Setup(l => l["VerifyClickLink"]).Returns(new LocalizedString("VerifyClickLink", "Click"));
            _mockLocalizer.Setup(l => l["VerifyLinkExpiries"]).Returns(new LocalizedString("VerifyLinkExpiries", "Expires"));
            _mockLocalizer.Setup(l => l["VerifyIgnoreIfWrong"]).Returns(new LocalizedString("VerifyIgnoreIfWrong", "Ignore"));

            var service = new EmailService(_mockConfig.Object, _mockLocalizer.Object);
            var language = "eu";

            // Act
            await service.SendVerificationEmailAsync("test@example.com", "testuser", "token123", language);

            // Assert
            Assert.Equal(language, CultureInfo.CurrentUICulture.Name);
            _mockLocalizer.Verify(l => l["VerifySubject"], Times.Once);
        }

        [Fact]
        public async Task SendDeactivationEmailAsync_WorksInMockMode()
        {
            // Arrange
            var section = new Mock<IConfigurationSection>();
            section.Setup(s => s.Value).Returns("true");
            _mockConfig.Setup(c => c.GetSection("EmailSettings:UseMockService")).Returns(section.Object);

            var service = new EmailService(_mockConfig.Object, _mockLocalizer.Object);

            // Act
            var exception = await Record.ExceptionAsync(() => 
                service.SendDeactivationEmailAsync("test@example.com", "testuser", "token123"));

            // Assert
            Assert.Null(exception);
        }
    }
}
