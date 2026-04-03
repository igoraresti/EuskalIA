using EuskalIA.Server.Models;

namespace EuskalIA.Server.Services.Auth
{
    /// <summary>
    /// Interface for JSON Web Token (JWT) services, used for user authentication and authorization.
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// Generates a signed JWT token for the specified user containing their identity and roles.
        /// </summary>
        /// <param name="user">The user for whom to generate the token.</param>
        /// <returns>A signed JWT token string.</returns>
        string GenerateToken(User user);
    }
}
