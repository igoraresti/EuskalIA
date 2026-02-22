using EuskalIA.Server.Models;

namespace EuskalIA.Server.Services.Auth
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
