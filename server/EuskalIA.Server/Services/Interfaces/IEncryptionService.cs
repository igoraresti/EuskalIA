namespace EuskalIA.Server.Services.Interfaces
{
    public interface IEncryptionService
    {
        string Encrypt(string text);
        string Decrypt(string cipherText);
    }
}
