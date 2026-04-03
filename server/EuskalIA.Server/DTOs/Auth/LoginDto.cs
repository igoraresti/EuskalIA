namespace EuskalIA.Server.DTOs.Auth
{
    /// <summary>
    /// Data Transfer Object for user login credentials.
    /// </summary>
    public class LoginDto
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
