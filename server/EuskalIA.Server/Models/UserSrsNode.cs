using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EuskalIA.Server.Models
{
    public class UserSrsNode
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required]
        [MaxLength(100)]
        public string ConceptId { get; set; } = string.Empty; // e.g., "NORK", "NONDIK"

        [Range(0.0, 5.0)]
        public float MasteryLevel { get; set; } = 0.0f;

        public float RiskFactor { get; set; } = 0.0f;

        public DateTime? LastReviewDate { get; set; }
        public DateTime? NextReviewDate { get; set; }
    }
}
