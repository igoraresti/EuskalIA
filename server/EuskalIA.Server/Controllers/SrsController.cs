using EuskalIA.Server.Data;
using EuskalIA.Server.DTOs.Srs;
using EuskalIA.Server.Models;
using EuskalIA.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EuskalIA.Server.Controllers
{
    /// <summary>
    /// Controller for managing the Spaced Repetition System (SRS) review sessions and node updates.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/euskalia/[controller]")]
    public class SrsController : ControllerBase
    {
        private readonly ISrsService _srsService;
        private readonly AppDbContext _context;
        private readonly ILogger<SrsController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="SrsController"/> class.
        /// </summary>
        /// <param name="srsService">The SRS service for handling review logic.</param>
        /// <param name="context">The database context.</param>
        /// <param name="logger">The controller logger.</param>
        public SrsController(ISrsService srsService, AppDbContext context, ILogger<SrsController> logger)
        {
            _srsService = srsService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves the current SRS status for the user (e.g., number of pending reviews).
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>An object containing the pending review count.</returns>
        [HttpGet("status/{userId}")]
        public async Task<ActionResult<object>> GetStatus(int userId)
        {
            var count = await _srsService.GetPendingReviewsCountAsync(userId);
            return Ok(new { pendingCount = count });
        }

        /// <summary>
        /// Generates a review session for the user containing exercises for due topics.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A collection of exercises selected for review.</returns>
        [HttpGet("session/{userId}")]
        public async Task<ActionResult<IEnumerable<Exercise>>> GetReviewSession(int userId)
        {
            _logger.LogInformation("Creating review session for user {UserId}.", userId);
            var dueNodes = await _srsService.GetDueNodesAsync(userId);
            if (!dueNodes.Any()) return Ok(new List<Exercise>());

            var topics = dueNodes.Select(n => n.ConceptId).ToList();

            // Fetch exercises from static lessons that match these topics
            var exercises = await _context.Exercises
                .Include(e => e.Lesson)
                .Where(e => e.Lesson != null && topics.Contains(e.Lesson.Topic))
                .OrderBy(x => Guid.NewGuid()) // Random shuffle
                .Take(15) // Limit review session size
                .ToListAsync();

            return Ok(exercises);
        }

        /// <summary>
        /// Records the result of a single SRS review attempt (Correct/Incorrect).
        /// Updates the internal SRS node for the given topic.
        /// </summary>
        /// <param name="dto">The review result details.</param>
        /// <returns>An <see cref="IActionResult"/> indicating success.</returns>
        [HttpPost("record")]
        public async Task<IActionResult> RecordReviewResult([FromBody] SrsReviewResultDto dto)
        {
            _logger.LogInformation("Recording SRS review for user {UserId} on topic {Topic}. Correct: {IsCorrect}.", dto.UserId, dto.Topic, dto.IsCorrect);
            await _srsService.UpdateSrsNodeAsync(dto.UserId, dto.Topic, dto.IsCorrect);
            return Ok();
        }

        // New endpoint to handle lesson completion and initial SRS node creation
        /// <summary>
        /// Signals that a lesson has been completed and creates/updates the corresponding SRS node.
        /// </summary>
        /// <param name="dto">The lesson completion details.</param>
        /// <returns>An <see cref="IActionResult"/> indicating success.</returns>
        [HttpPost("complete-lesson")]
        public async Task<IActionResult> CompleteLesson([FromBody] LessonCompletionDto dto)
        {
            var lesson = await _context.Lessons.FindAsync(dto.LessonId);
            if (lesson == null) return NotFound("Lesson not found");

            // Update SRS for this topic
            // In a lesson, we consider it "Correct" if the user passed with a certain threshold,
            // but for simplicity, finishing a lesson creates the SRS node.
            await _srsService.UpdateSrsNodeAsync(dto.UserId, lesson.Topic, true);

            return Ok();
        }
    }
}
