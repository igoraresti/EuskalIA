namespace EuskalIA.Server.Services.AI
{
    public class GeminiSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "gemini-1.5-flash";
        public double Temperature { get; set; } = 0.7;
    }
}
