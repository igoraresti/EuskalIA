using System.IO;
using System.Security.Cryptography;
using System.Text;
using EuskalIA.Server.Services.Interfaces;

namespace EuskalIA.Server.Services
{
    public class EncryptionService : IEncryptionService
    {
        private static readonly string Key = "EuskalIA_Secret_Key_2024_Security"; // 32 chars
        private static readonly byte[] KeyBytes = Encoding.UTF8.GetBytes(Key.Substring(0, 32));
        private static readonly byte[] IvBytes = Encoding.UTF8.GetBytes(Key.Substring(0, 16));

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
            catch
            {
                return cipherText; // Return original if decryption fails (e.g. not encrypted yet)
            }
        }
    }
}
