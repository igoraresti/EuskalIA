using System.ComponentModel.DataAnnotations;

namespace EuskalIA.Server.Models
{
    public class Achievement
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty; // e.g., "7_DAY_STREAK"
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string Icon { get; set; } = "Award"; // Lucide icon name
        
        [Required]
        public string Category { get; set; } = "XP"; // XP, STREAK, LESSONS
        
        [Required]
        public int TargetValue { get; set; }
    }
}
