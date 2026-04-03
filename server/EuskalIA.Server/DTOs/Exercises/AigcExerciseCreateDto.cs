namespace EuskalIA.Server.DTOs.Exercises
{
    /// <summary>
    /// Data Transfer Object for initiating the AI generation of a new set of exercises.
    /// </summary>
    public class AigcExerciseCreateDto
    {
        public string ExerciseCode { get; set; } = string.Empty;
        public string TemplateType { get; set; } = string.Empty;
        public string LevelId { get; set; } = string.Empty;
        public string Topics { get; set; } = string.Empty;
        public int Difficulty { get; set; }
        public string JsonSchema { get; set; } = string.Empty;
        public int Count { get; set; } = 3;
        
        /// <summary>
        /// Optional manual context. If not provided, it will be extracted from the lesson books.
        /// </summary>
        public string? ManualContext { get; set; }
    }
}
