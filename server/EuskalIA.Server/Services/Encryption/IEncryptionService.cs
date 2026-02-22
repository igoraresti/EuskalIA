namespace EuskalIA.Server.Services.Encryption
{
    public interface IEncryptionService
    {
        string Encrypt(string text);
        string Decrypt(string cipherText);
    }
}
