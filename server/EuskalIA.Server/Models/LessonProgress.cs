using System;
using System.Text.Json.Serialization;

namespace EuskalIA.Server.Models
{
    public class LessonProgress
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int LessonId { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        public User? User { get; set; }
        [JsonIgnore]
        public Lesson? Lesson { get; set; }
    }
}
