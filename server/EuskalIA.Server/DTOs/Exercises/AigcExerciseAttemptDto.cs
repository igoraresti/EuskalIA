namespace EuskalIA.Server.DTOs.Exercises
{
    /// <summary>
    /// Data Transfer Object for recording an attempt at an AIGC exercise.
    /// </summary>
    public class AigcExerciseAttemptDto
    {
        public int UserId { get; set; }
        public Guid ExerciseId { get; set; }
        public bool IsCorrect { get; set; }
    }
}
