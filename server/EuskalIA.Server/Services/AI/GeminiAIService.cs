using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using EuskalIA.Server.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EuskalIA.Server.Services.AI
{
    public class GeminiAIService : IAIService
    {
        private readonly HttpClient _httpClient;
        private readonly GeminiSettings _settings;
        private readonly ILogger<GeminiAIService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public GeminiAIService(
            HttpClient httpClient, 
            IOptions<GeminiSettings> settings, 
            ILogger<GeminiAIService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        private async Task LogAsync(string levelId, string operation, string status, string message)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<EuskalIA.Server.Data.AppDbContext>();
                
                var log = new AigcLog
                {
                    LevelId = levelId,
                    Operation = operation,
                    Status = status,
                    Message = message,
                    ModelUsed = _settings.Model,
                    Timestamp = DateTime.UtcNow
                };

                context.AigcLogs.Add(log);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save AI log to database.");
            }
        }

        public Task<List<Exercise>> GenerateExercisesAsync(string topic, int count)
        {
            // Note: IAIService interface uses the old Exercise model. 
            // We'll need to adapt it or create a new method for AIGC.
            // For now, let's implement the core generation logic for AIGC.
            return Task.FromResult(new List<Exercise>()); // Legacy support
        }

        public async Task<List<AigcExercise>> GenerateAigcExercisesAsync(string levelId, string context, int count)
        {
            if (string.IsNullOrEmpty(_settings.ApiKey))
            {
                await LogAsync(levelId, "Generation", "ERROR", "Missing API Key.");
                return new List<AigcExercise>();
            }

            _logger.LogInformation("Generating {Count} exercises for level {LevelId} with Gemini model {Model}.", count, levelId, _settings.Model);

            var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_settings.Model}:generateContent?key={_settings.ApiKey}";

            var systemPrompt = @"Tu eres un profesor experto en Euskera. Tu objetivo es generar ejercicios educativos basados EXCLUSIVAMENTE en el texto de contexto proporcionado.
Debes devolver un array JSON de objetos que sigan exactamente esta estructura:
{
    ""exerciseCode"": ""string_unico"",
    ""templateType"": ""multiple_choice"" o ""block_builder"",
    ""levelId"": ""string_del_nivel"",
    ""topics"": ""string_de_temas"",
    ""difficulty"": 1-5,
    ""jsonSchema"": ""string_json_con_la_definicion_del_ejercicio""
}

Para 'multiple_choice', el jsonSchema debe ser:
{
    ""question"": { ""es"": """", ""en"": """", ""fr"": """", ""pl"": """" },
    ""options"": [""opcion1"", ""opcion2"", ""opcion3"", ""opcion4""],
    ""correctAnswer"": ""opcion_correcta""
}

Para 'block_builder', el jsonSchema debe ser:
{
    ""promptLocal"": { ""es"": """", ""en"": """", ""fr"": """", ""pl"": """" },
    ""validation"": {
        ""correctSequence"": [""id1"", ""id2""],
        ""targetWord"": """",
        ""targetTranslation"": { ""es"": """", ""en"": """", ""eu"": """", ""fr"": """", ""pl"": """" },
        ""feedback"": { ""onSuccess"": { ""es"": ""¡Perfecto!"", ... }, ""onFail"": { ""es"": ""..."", ... } }
    },
    ""elements"": {
        ""stem"": { ""id"": ""..."", ""text"": ""..."", ""type"": ""noun|verb"", ""colorCode"": ""#..."" },
        ""pieces"": [{ ""id"": ""..."", ""text"": ""..."", ""type"": ""article|suffix|wrong"", ""colorCode"": ""#..."" }]
    }
}

IMPORTANTE: El campo jsonSchema dentro del JSON principal DEBE SER UN STRING (un JSON escapado) no un objeto.
Solo devuelve el JSON, sin bloques de código ni explicaciones.";

            var userPrompt = $"Contexto del libro:\n{context}\n\nGenera {count} ejercicios para el nivel {levelId}.";

            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = systemPrompt + "\n\n" + userPrompt }
                        }
                    }
                }
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(url, content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Gemini API call failed with status {StatusCode}: {ErrorBody}", response.StatusCode, errorBody);
                    string status = response.StatusCode == System.Net.HttpStatusCode.TooManyRequests ? "QUOTA_EXHAUSTED" : "ERROR";
                    await LogAsync(levelId, "Generation", status, $"HTTP {(int)response.StatusCode}: {errorBody}");
                    return new List<AigcExercise>();
                }

                var responseBody = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseBody, options);

                var aiText = geminiResponse?.Candidates?[0].Content.Parts[0].Text;
                if (string.IsNullOrEmpty(aiText))
                {
                    await LogAsync(levelId, "Generation", "ERROR", "Empty response from Gemini.");
                    return new List<AigcExercise>();
                }

                // Clean-up AI response (remove markdown blocks if Gemini added them)
                if (aiText.StartsWith("```json")) aiText = aiText.Replace("```json", "");
                if (aiText.EndsWith("```")) aiText = aiText.Substring(0, aiText.Length - 3);
                aiText = aiText.Trim();

                var exercises = JsonSerializer.Deserialize<List<AigcExercise>>(aiText, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                await LogAsync(levelId, "Generation", "SUCCESS", $"Generated {exercises?.Count ?? 0} exercises.");
                return exercises ?? new List<AigcExercise>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GeminiAIService while generating exercises for level {LevelId}.", levelId);
                await LogAsync(levelId, "Generation", "ERROR", ex.Message);
                return new List<AigcExercise>();
            }
        }

        public class GeminiResponse
        {
            public List<Candidate>? Candidates { get; set; }
        }

        public class Candidate
        {
            public Content? Content { get; set; }
        }

        public class Content
        {
            public List<Part>? Parts { get; set; }
        }

        public class Part
        {
            public string? Text { get; set; }
        }
    }
}
