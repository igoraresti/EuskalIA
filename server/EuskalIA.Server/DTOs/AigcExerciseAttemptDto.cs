namespace EuskalIA.Server.DTOs
{
    public class AigcExerciseAttemptDto
    {
        public int UserId { get; set; }
        public Guid ExerciseId { get; set; }
        public bool IsCorrect { get; set; }
    }
}
