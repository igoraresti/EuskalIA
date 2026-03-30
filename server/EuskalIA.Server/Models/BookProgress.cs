using System.ComponentModel.DataAnnotations;

namespace EuskalIA.Server.Models
{
    public class BookProgress
    {
        [Key]
        [MaxLength(50)]
        public string LevelId { get; set; } = string.Empty; // A1, A2, B1

        [MaxLength(255)]
        public string BookName { get; set; } = string.Empty;

        public int LastPageProcessed { get; set; } = 0;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
