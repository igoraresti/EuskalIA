using EuskalIA.Server.Services;
using Xunit;

namespace EuskalIA.Tests.Services
{
    public class EncryptionServiceTests
    {
        [Fact]
        public void EncryptionService_EncryptsAndDecrypts()
        {
            var service = new EncryptionService();
            var original = "Hello World";
            var encrypted = service.Encrypt(original);
            var decrypted = service.Decrypt(encrypted);
            
            Assert.NotEqual(original, encrypted);
            Assert.Equal(original, decrypted);
        }
    }
}
