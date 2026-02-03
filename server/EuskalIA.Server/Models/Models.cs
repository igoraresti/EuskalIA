using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EuskalIA.Server.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty; // Full Name
        public string Email { get; set; } = string.Empty; // Encrypted
        public string Nickname { get; set; } = string.Empty; // Encrypted
        public string Password { get; set; } = string.Empty; // Encrypted
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        
        [JsonIgnore]
        public Progress? Progress { get; set; }
    }

    public class Progress
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int XP { get; set; }
        public int WeeklyXP { get; set; }
        public int MonthlyXP { get; set; }
        public int Streak { get; set; }
        public int Level { get; set; }
        public int Txanponak { get; set; }
        public DateTime LastLessonDate { get; set; } = DateTime.MinValue;
        public string LastLessonTitle { get; set; } = string.Empty;
        
        [JsonIgnore]
        public User? User { get; set; }
    }

    public class Lesson
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public int Level { get; set; } // 1 = A1, 2 = A2, etc.
        public List<Exercise> Exercises { get; set; } = new();
    }

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
