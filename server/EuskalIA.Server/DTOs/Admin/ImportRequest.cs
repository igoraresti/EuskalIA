namespace EuskalIA.Server.DTOs.Admin
{
    /// <summary>
    /// Request model for the exercise import process with duplicate detection.
    /// </summary>
    /// <param name="Exercises">List of exercises to process.</param>
    /// <param name="Confirm">Whether to proceed with actual insertion.</param>
    /// <param name="Threshold">Similarity threshold (0.0 to 1.0).</param>
    public record ImportRequest(
        List<ImportExerciseItem> Exercises,
        bool Confirm,
        double Threshold
    );
}
