using System.Text.Json.Serialization;

namespace EuskalIA.Server.DTOs
{
    public class RegisterDto
    {
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;
        
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
        
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
        
        [JsonPropertyName("language")]
        public string? Language { get; set; } = "es"; // Default: Spanish
    }
}
