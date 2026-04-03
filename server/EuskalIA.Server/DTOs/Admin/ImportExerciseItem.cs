namespace EuskalIA.Server.DTOs.Admin
{
    /// <summary>
    /// Model representing an exercise item to be imported.
    /// </summary>
    /// <param name="TemplateType">The exercise format (e.g., "multiple_choice").</param>
    /// <param name="LevelId">The difficulty level.</param>
    /// <param name="Topics">Comma-separated topics.</param>
    /// <param name="Difficulty">The difficulty score (1-5).</param>
    /// <param name="JsonSchema">The full exercise definition in JSON.</param>
    public record ImportExerciseItem(
        string TemplateType,
        string LevelId,
        string Topics,
        int Difficulty,
        string JsonSchema
    );
}
