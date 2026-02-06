using System.Collections.Generic;

namespace EuskalIA.Server.Models
{
    public class Lesson
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public int Level { get; set; } // 1 = A1, 2 = A2, etc.
        public List<Exercise> Exercises { get; set; } = new();
    }
}
