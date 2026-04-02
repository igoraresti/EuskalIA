using EuskalIA.Server.Services.Email;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Localization;
using Moq;
using Microsoft.Extensions.Logging;
using System.Globalization;
using Xunit;

namespace EuskalIA.Tests.Services
{
    public class EmailServiceTests
    {
        private readonly Mock<IEmailQueue> _mockQueue;
        private readonly Mock<IStringLocalizer<EmailService>> _mockLocalizer;
        private readonly Mock<IWebHostEnvironment> _mockEnv;

        public EmailServiceTests()
        {
            _mockQueue = new Mock<IEmailQueue>();
            _mockLocalizer = new Mock<IStringLocalizer<EmailService>>();
            _mockEnv = new Mock<IWebHostEnvironment>();
            
            // Setup a dummy path for templates
            _mockEnv.Setup(e => e.ContentRootPath).Returns(Directory.GetCurrentDirectory());
            
            // Ensure Templates directory exists locally for tests if needed, 
            // but we'll mock the File IO or just ensure the test environment is sane.
            // For unit tests, we usually mock the filesystem, but here EmailService uses File.ReadAllTextAsync.
            // We'll create a dummy template file in the test execution directory.
            var templateDir = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Emails");
            Directory.CreateDirectory(templateDir);
            File.WriteAllText(Path.Combine(templateDir, "VerificationTemplate.html"), "<html>{{Title}} {{Greeting}} {{VerificationUrl}}</html>");
            File.WriteAllText(Path.Combine(templateDir, "DeactivationTemplate.html"), "<html>{{Title}} {{Greeting}} {{DeactivationUrl}}</html>");
        }

        [Fact]
        public async Task SendVerificationEmailAsync_EnqueuesLocalizedEmail()
        {
            // Arrange
            var localizedString = new LocalizedString("VerifySubject", "Subject");
            _mockLocalizer.Setup(l => l["VerifySubject"]).Returns(localizedString);
            _mockLocalizer.Setup(l => l["VerifyGreeting"]).Returns(new LocalizedString("VerifyGreeting", "Hello {0}"));
            _mockLocalizer.Setup(l => l["VerifyThanks"]).Returns(new LocalizedString("VerifyThanks", "Thanks"));
            _mockLocalizer.Setup(l => l["VerifyClickLink"]).Returns(new LocalizedString("VerifyClickLink", "Click"));
            _mockLocalizer.Setup(l => l["VerifyLinkExpiries"]).Returns(new LocalizedString("VerifyLinkExpiries", "Expires"));
            _mockLocalizer.Setup(l => l["VerifyIgnoreIfWrong"]).Returns(new LocalizedString("VerifyIgnoreIfWrong", "Ignore"));

            var mockLogger = new Mock<ILogger<EmailService>>();
            var service = new EmailService(_mockQueue.Object, _mockLocalizer.Object, _mockEnv.Object, mockLogger.Object);
            var language = "eu";

            // Act
            await service.SendVerificationEmailAsync("test@example.com", "testuser", "token123", language);

            // Assert
            _mockQueue.Verify(q => q.EnqueueAsync(It.Is<EmailMessage>(m => 
                m.ToEmail == "test@example.com" && 
                m.Language == language &&
                m.Subject == "Subject"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SendDeactivationEmailAsync_EnqueuesEmail()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<EmailService>>();
            var service = new EmailService(_mockQueue.Object, _mockLocalizer.Object, _mockEnv.Object, mockLogger.Object);

            // Act
            await service.SendDeactivationEmailAsync("test@example.com", "testuser", "token123");

            // Assert
            _mockQueue.Verify(q => q.EnqueueAsync(It.Is<EmailMessage>(m => 
                m.ToEmail == "test@example.com" && 
                m.ToName == "testuser"), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
