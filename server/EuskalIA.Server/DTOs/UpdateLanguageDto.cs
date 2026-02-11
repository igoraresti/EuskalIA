using System.Text.Json.Serialization;

namespace EuskalIA.Server.DTOs
{
    public class UpdateLanguageDto
    {
        [JsonPropertyName("language")]
        public string Language { get; set; } = string.Empty;
    }
}
