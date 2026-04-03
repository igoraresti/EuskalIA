using EuskalIA.Server.Models;

namespace EuskalIA.Server.Services.AI
{
    /// <summary>
    /// Service for extracting educational context from PDF source materials based on user level.
    /// Manages reading progress to ensure content is not repeated unnecessarily.
    /// </summary>
    public interface IKnowledgeService
    {
        /// <summary>
        /// Retrieves the next chunk of context for a given level, extracts the text, and updates progress.
        /// </summary>
        /// <param name="levelId">The difficulty level identifier (e.g., "A1").</param>
        /// <returns>A tuple containing the extracted text content, the book name, and the page number.</returns>
        Task<(string Content, string BookName, int PageNumber)> GetNextContextAsync(string levelId);
    }
}
