using EuskalIA.Server.Models;

namespace EuskalIA.Server.Services.Auth
{
    /// <summary>
    /// Interface for social authentication services that validate external OAuth tokens.
    /// Supports multiple providers such as Google and Facebook.
    /// </summary>
    public interface ISocialAuthService
    {
        /// <summary>
        /// Validates a Google ID token and returns the corresponding user profile if valid.
        /// </summary>
        /// <param name="token">The Google ID token strings.</param>
        /// <returns>A <see cref="User"/> object if validation succeeds; otherwise, null.</returns>
        Task<User?> ValidateGoogleTokenAsync(string token);

        /// <summary>
        /// Validates a Facebook access token and returns the corresponding user profile if valid.
        /// </summary>
        /// <param name="token">The Facebook access token string.</param>
        /// <returns>A <see cref="User"/> object if validation succeeds; otherwise, null.</returns>
        Task<User?> ValidateFacebookTokenAsync(string token);
    }
}
