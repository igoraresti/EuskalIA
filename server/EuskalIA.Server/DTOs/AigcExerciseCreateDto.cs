using System.ComponentModel.DataAnnotations;

namespace EuskalIA.Server.DTOs
{
    public class AigcExerciseCreateDto
    {
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
        public string Topics { get; set; } = string.Empty;

        public int Difficulty { get; set; }

        [Required]
        public string JsonSchema { get; set; } = string.Empty;
    }
}
