using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
namespace EuskalIA.Server.Services.Encryption
{
    public class EncryptionService : IEncryptionService
    {
        private readonly ILogger<EncryptionService> _logger;
        private static readonly string Key = "EuskalIA_Secret_Key_2024_Security"; // 32 chars
        private static readonly byte[] KeyBytes = Encoding.UTF8.GetBytes(Key.Substring(0, 32));
        private static readonly byte[] IvBytes = Encoding.UTF8.GetBytes(Key.Substring(0, 16));

        public EncryptionService(ILogger<EncryptionService> logger)
        {
            _logger = logger;
        }

        public string Encrypt(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            using (var aes = Aes.Create())
            {
                aes.Key = KeyBytes;
                aes.IV = IvBytes;

                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(text);
                        }
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;

            try 
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = KeyBytes;
                    aes.IV = IvBytes;

                    var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (var msDecrypt = new MemoryStream(Convert.FromBase64String(cipherText)))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Decryption failed. Returning original cipherText.");
                return cipherText; // Return original if decryption fails (e.g. not encrypted yet)
            }
        }
    }
}
