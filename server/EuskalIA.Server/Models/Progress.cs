using System;
using System.Text.Json.Serialization;

namespace EuskalIA.Server.Models
{
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
}
