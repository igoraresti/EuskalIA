namespace EuskalIA.Server.DTOs
{
    public class SocialLoginDto
    {
        public string Token { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty; // "google" or "facebook"
        public string? Language { get; set; }
    }
}
