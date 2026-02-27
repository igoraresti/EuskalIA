using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EuskalIA.Server.Models
{
    public class UserExerciseAttempt
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public int UserId { get; set; }

        [Required]
        public Guid ExerciseId { get; set; }

        public bool IsCorrect { get; set; }

        public DateTime AttemptDate { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [ForeignKey("ExerciseId")]
        public AigcExercise? Exercise { get; set; }
    }
}
