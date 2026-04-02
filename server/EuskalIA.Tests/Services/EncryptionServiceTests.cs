using EuskalIA.Server.Services.Encryption;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;

namespace EuskalIA.Tests.Services
{
    public class EncryptionServiceTests
    {
        [Fact]
        public void EncryptionService_EncryptsAndDecrypts()
        {
            var mockLogger = new Mock<ILogger<EncryptionService>>();
            var service = new EncryptionService(mockLogger.Object);
            var original = "Hello World";
            var encrypted = service.Encrypt(original);
            var decrypted = service.Decrypt(encrypted);
            
            Assert.NotEqual(original, encrypted);
            Assert.Equal(original, decrypted);
        }
    }
}
