using System.ComponentModel.DataAnnotations;

namespace EuskalIA.Server.Models
{
    public class AigcLog
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [MaxLength(20)]
        public string LevelId { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Operation { get; set; } = "Generation"; // Generation, Parsing

        [MaxLength(20)]
        public string Status { get; set; } = "SUCCESS"; // SUCCESS, ERROR

        public string Message { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [MaxLength(100)]
        public string? ModelUsed { get; set; }
    }
}
