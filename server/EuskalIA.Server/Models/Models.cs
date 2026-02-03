using System;
using System.Collections.Generic;

namespace EuskalIA.Server.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Progress? Progress { get; set; }
    }

    public class Progress
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int XP { get; set; }
        public int Streak { get; set; }
        public int Level { get; set; }
        public int Txanponak { get; set; }
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
        public Lesson? Lesson { get; set; }
    }
}
