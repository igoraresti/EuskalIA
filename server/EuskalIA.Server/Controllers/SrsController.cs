using EuskalIA.Server.Data;
using EuskalIA.Server.DTOs;
using EuskalIA.Server.Models;
using EuskalIA.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EuskalIA.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/euskalia/[controller]")]
    public class SrsController : ControllerBase
    {
        private readonly ISrsService _srsService;
        private readonly AppDbContext _context;
        private readonly ILogger<SrsController> _logger;

        public SrsController(ISrsService srsService, AppDbContext context, ILogger<SrsController> logger)
        {
            _srsService = srsService;
            _context = context;
            _logger = logger;
        }

        [HttpGet("status/{userId}")]
        public async Task<ActionResult<object>> GetStatus(int userId)
        {
            var count = await _srsService.GetPendingReviewsCountAsync(userId);
            return Ok(new { pendingCount = count });
        }

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
                .Where(e => topics.Contains(e.Lesson.Topic))
                .OrderBy(x => Guid.NewGuid()) // Random shuffle
                .Take(15) // Limit review session size
                .ToListAsync();

            return Ok(exercises);
        }

        [HttpPost("record")]
        public async Task<IActionResult> RecordReviewResult([FromBody] SrsReviewResultDto dto)
        {
            _logger.LogInformation("Recording SRS review for user {UserId} on topic {Topic}. Correct: {IsCorrect}.", dto.UserId, dto.Topic, dto.IsCorrect);
            await _srsService.UpdateSrsNodeAsync(dto.UserId, dto.Topic, dto.IsCorrect);
            return Ok();
        }

        // New endpoint to handle lesson completion and initial SRS node creation
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

    public class SrsReviewResultDto
    {
        public int UserId { get; set; }
        public string Topic { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }

    public class LessonCompletionDto
    {
        public int UserId { get; set; }
        public int LessonId { get; set; }
    }
}
