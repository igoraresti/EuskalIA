using System.Text.Json.Serialization;

namespace EuskalIA.Server.Models
{
    public class Exercise
    {
        public int Id { get; set; }
        public int LessonId { get; set; }
        public string Type { get; set; } = "MultipleChoice"; // MultipleChoice, Translation, etc.
        public string Question { get; set; } = string.Empty;
        public string CorrectAnswer { get; set; } = string.Empty;
        public string OptionsJson { get; set; } = string.Empty; // JSON array of options
        
        [JsonIgnore]
        public Lesson? Lesson { get; set; }
    }
}
