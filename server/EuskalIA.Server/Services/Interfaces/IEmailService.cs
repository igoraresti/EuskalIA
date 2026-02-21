namespace EuskalIA.Server.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendVerificationEmailAsync(string email, string username, string token, string language);
        Task SendDeactivationEmailAsync(string email, string username, string token);
    }
}
