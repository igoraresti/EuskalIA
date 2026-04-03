using System.Text;
using EuskalIA.Server.Data;
using EuskalIA.Server.Models;
using Microsoft.EntityFrameworkCore;
using UglyToad.PdfPig;

namespace EuskalIA.Server.Services.AI
{
    /// <summary>
    /// Implementation of <see cref="IKnowledgeService"/> that uses PdfPig to extract text from local PDF lessons.
    /// Tracks reading progress in the database to provide sequential learning material.
    /// </summary>
    public class KnowledgeService : IKnowledgeService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<KnowledgeService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="KnowledgeService"/> class.
        /// </summary>
        /// <param name="context">The database context for tracking page progress.</param>
        /// <param name="env">The web host environment for resolving local PDF paths.</param>
        /// <param name="logger">The service logger.</param>
        public KnowledgeService(AppDbContext context, IWebHostEnvironment env, ILogger<KnowledgeService> logger)
        {
            _context = context;
            _env = env;
            _logger = logger;
        }

        /// <summary>
        /// Extracts text content from the next unprocessed page of the mapped book for the specified level.
        /// Updates the <see cref="BookProgress"/> to point to subsequent pages for future calls.
        /// </summary>
        /// <param name="levelId">The targeted language level.</param>
        /// <returns>Extracted text for AI consumption and source metadata.</returns>
        public async Task<(string Content, string BookName, int PageNumber)> GetNextContextAsync(string levelId)
        {
            _logger.LogInformation("Retrieving next context for level {LevelId}.", levelId);
            var bookName = MapLevelToBook(levelId);

            var progress = await _context.BookProgresses
                .FirstOrDefaultAsync(p => p.LevelId == levelId);

            if (progress == null)
            {
                progress = new BookProgress { LevelId = levelId, BookName = bookName, LastPageProcessed = 0 };
                _context.BookProgresses.Add(progress);
                await _context.SaveChangesAsync();
            }

            var filePath = Path.Combine(_env.ContentRootPath, "..", "..", "Lessons", bookName);

            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Book not found at path: {FilePath}. Falling back to default context.", filePath);
                return ("No context available. Generate general Euskara exercises.", "None", 0);
            }

            int targetPage = progress.LastPageProcessed + 1;
            string extractedText = "";

            try
            {
                using (var document = PdfDocument.Open(filePath))
                {
                    // If target page exceeds document length, loop back or stay at end
                    if (targetPage > document.NumberOfPages)
                    {
                        targetPage = 1; 
                    }

                    // Extract 2 pages for better context (or just 1 if we want to be granular)
                    var sb = new StringBuilder();
                    var page = document.GetPage(targetPage);
                    sb.AppendLine(page.Text);
                    
                    if (targetPage + 1 <= document.NumberOfPages)
                    {
                        var nextPage = document.GetPage(targetPage + 1);
                        sb.AppendLine(nextPage.Text);
                    }

                    extractedText = sb.ToString();
                    
                    // Update progress
                    progress.LastPageProcessed = targetPage + 1; // Mark both as "seen" or just move one
                    progress.LastUpdated = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Successfully extracted context from {BookName}, starting at page {PageNumber}.", bookName, targetPage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text from PDF {BookName} at page {PageNumber}.", bookName, targetPage);
                return ("Error extracting context.", bookName, targetPage);
            }

            return (extractedText, bookName, targetPage);
        }

        /// <summary>
        /// Maps a standardized level ID to the specific filename of the PDF coursebook.
        /// </summary>
        /// <param name="levelId">The level identifier.</param>
        /// <returns>The filename of the PDF book.</returns>
        private string MapLevelToBook(string levelId)
        {
            return levelId.ToUpper() switch
            {
                "A1" => "EUSKERA BASICO COMUN.pdf",
                "B1" => "B1 ikastaroaren apunteak.pdf",
                "A2" => "bk_poni_000040.pdf",
                _ => "EUSKERA BASICO COMUN.pdf"
            };
        }
    }
}
