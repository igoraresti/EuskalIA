namespace EuskalIA.Server.DTOs.Exercises
{
    /// <summary>
    /// Data Transfer Object for returning AI-generated exercises to the frontend.
    /// </summary>
    public class AigcExerciseResponseDto
    {
        public Guid Id { get; set; }
        public string ExerciseCode { get; set; } = string.Empty;
        public string TemplateType { get; set; } = string.Empty;
        public string LevelId { get; set; } = string.Empty;
        public string Topics { get; set; } = string.Empty;
        public int Difficulty { get; set; }
        public string Status { get; set; } = string.Empty;
        public string JsonSchema { get; set; } = string.Empty;
    }
}
