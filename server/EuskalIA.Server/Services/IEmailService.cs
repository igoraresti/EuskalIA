namespace EuskalIA.Server.Services
{
    public interface IEmailService
    {
        Task SendVerificationEmailAsync(string email, string username, string token);
    }
}
