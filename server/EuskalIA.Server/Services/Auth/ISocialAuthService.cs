using EuskalIA.Server.Models;

namespace EuskalIA.Server.Services.Auth
{
    public interface ISocialAuthService
    {
        Task<User?> ValidateGoogleTokenAsync(string token);
        Task<User?> ValidateFacebookTokenAsync(string token);
    }
}
