using EuskalIA.Server.Models;

namespace EuskalIA.Server.Services.AI
{
    /// <summary>
    /// Interface for AI services that handle the generation of language exercises and AI-generated content.
    /// </summary>
    public interface IAIService
    {
        /// <summary>
        /// Generates a set of language exercises based on a specific topic.
        /// Used for traditional lesson content.
        /// </summary>
        /// <param name="topic">The topic (e.g., "Food", "Travel").</param>
        /// <param name="count">The number of exercises to generate.</param>
        /// <returns>A list of generated <see cref="Exercise"/> objects.</returns>
        Task<List<Exercise>> GenerateExercisesAsync(string topic, int count);

        /// <summary>
        /// Generates complex AIGC exercises based on a specific level and cultural context.
        /// Used for dynamic practice sessions based on real text extraction.
        /// </summary>
        /// <param name="levelId">The target difficulty level (e.g., "A1").</param>
        /// <param name="context">The source text context extracted from PDFs or articles.</param>
        /// <param name="count">The number of exercises to generate.</param>
        /// <returns>A list of generated <see cref="AigcExercise"/> objects.</returns>
        Task<List<AigcExercise>> GenerateAigcExercisesAsync(string levelId, string context, int count);
    }
}
