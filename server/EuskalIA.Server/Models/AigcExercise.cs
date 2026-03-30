using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EuskalIA.Server.Models
{
    public class AigcExercise
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(100)]
        public string ExerciseCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string TemplateType { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LevelId { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Topics { get; set; } = string.Empty; // Store as comma-separated or JSON

        public int Difficulty { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "BETA"; // BETA, VERIFIED, REJECTED

        [Required]
        public string JsonSchema { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? SourceMaterial { get; set; } // Book filename
        
        public int? SourcePage { get; set; } // Page number in the book

        public float SuccessRate { get; set; } = 0.0f;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
