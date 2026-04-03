namespace EuskalIA.Server.DTOs.Auth
{
    /// <summary>
    /// Data Transfer Object for authentication via an external social provider.
    /// </summary>
    public class SocialLoginDto
    {
        public string Provider { get; set; } = string.Empty; // "GOOGLE" or "FACEBOOK"
        public string Token { get; set; } = string.Empty;
        public string? Language { get; set; }
    }
}
