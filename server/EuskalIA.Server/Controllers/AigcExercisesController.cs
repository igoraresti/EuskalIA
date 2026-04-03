using EuskalIA.Server.Data;
using EuskalIA.Server.DTOs.Exercises;
using EuskalIA.Server.Models;
using EuskalIA.Server.Services.AI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EuskalIA.Server.Controllers
{
    /// <summary>
    /// Controller for managing AI-Generated Content (AIGC) exercises, session generation, and user attempts.
    /// </summary>
    [ApiController]
    [Route("api/euskalia/[controller]")]
    public class AigcExercisesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAIService _aiService;
        private readonly IKnowledgeService _knowledgeService;
        private readonly ILogger<AigcExercisesController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AigcExercisesController"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="aiService">The AI service for generating complex exercises.</param>
        /// <param name="knowledgeService">The knowledge service for context extraction from PDFs.</param>
        /// <param name="logger">The controller logger.</param>
        public AigcExercisesController(AppDbContext context, IAIService aiService, IKnowledgeService knowledgeService, ILogger<AigcExercisesController> logger)
        {
            _context = context;
            _aiService = aiService;
            _knowledgeService = knowledgeService;
            _logger = logger;
        }

        // GET: api/aigcexercises?levelId=A1
        /// <summary>
        /// Retrieves a list of approved exercises filtered by level.
        /// </summary>
        /// <param name="levelId">The level identifier (e.g., "A1").</param>
        /// <returns>A collection of <see cref="AigcExerciseResponseDto"/> objects.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AigcExerciseResponseDto>>> GetExercises([FromQuery] string levelId)
        {
            var query = _context.AigcExercises.AsQueryable();

            if (!string.IsNullOrEmpty(levelId))
            {
                query = query.Where(e => e.LevelId == levelId);
            }

            // Only serve APPROVED exercises to players
            query = query.Where(e => e.Status == "APPROVED");

            var exercises = await query
                .Select(e => new AigcExerciseResponseDto
                {
                    Id = e.Id,
                    ExerciseCode = e.ExerciseCode,
                    TemplateType = e.TemplateType,
                    LevelId = e.LevelId,
                    Topics = e.Topics,
                    Difficulty = e.Difficulty,
                    Status = e.Status,
                    JsonSchema = e.JsonSchema
                })
                .ToListAsync();

            return Ok(exercises);
        }

        // GET: api/aigcexercises/5
        /// <summary>
        /// Retrieves a specific exercise by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the exercise.</param>
        /// <returns>An <see cref="AigcExerciseResponseDto"/> if found; otherwise, NotFound.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<AigcExerciseResponseDto>> GetExercise(Guid id)
        {
            var exercise = await _context.AigcExercises.FindAsync(id);

            if (exercise == null)
            {
                return NotFound();
            }

            return new AigcExerciseResponseDto
            {
                Id = exercise.Id,
                ExerciseCode = exercise.ExerciseCode,
                TemplateType = exercise.TemplateType,
                LevelId = exercise.LevelId,
                Topics = exercise.Topics,
                Difficulty = exercise.Difficulty,
                Status = exercise.Status,
                JsonSchema = exercise.JsonSchema
            };
        }

        // POST: api/aigcexercises
        /// <summary>
        /// Manually creates a new AIGC exercise (typically used for testing or seeding).
        /// New exercises start with "BETA" status.
        /// </summary>
        /// <param name="dto">The exercise data.</param>
        /// <returns>The created exercise response.</returns>
        [HttpPost]
        public async Task<ActionResult<AigcExerciseResponseDto>> CreateExercise(AigcExerciseCreateDto dto)
        {
            var exercise = new AigcExercise
            {
                ExerciseCode = dto.ExerciseCode,
                TemplateType = dto.TemplateType,
                LevelId = dto.LevelId,
                Topics = dto.Topics,
                Difficulty = dto.Difficulty,
                JsonSchema = dto.JsonSchema,
                Status = "BETA", // Every new exercise starts in Shadow Testing
                CreatedAt = DateTime.UtcNow
            };

            _context.AigcExercises.Add(exercise);
            await _context.SaveChangesAsync();

            var responseDto = new AigcExerciseResponseDto
            {
                Id = exercise.Id,
                ExerciseCode = exercise.ExerciseCode,
                TemplateType = exercise.TemplateType,
                LevelId = exercise.LevelId,
                Topics = exercise.Topics,
                Difficulty = exercise.Difficulty,
                Status = exercise.Status,
                JsonSchema = exercise.JsonSchema
            };

            return CreatedAtAction(nameof(GetExercise), new { id = exercise.Id }, responseDto);
        }

        // GET: api/aigcexercises/session?levelId=B1&userId=1
        /// <summary>
        /// Generates a personalized practice session for a user.
        /// Mixes recently failed exercises, new unattempted exercises, and reviewed ones.
        /// Triggers background AI generation if the available exercise pool is low.
        /// </summary>
        /// <param name="levelId">The target difficulty level.</param>
        /// <param name="userId">The user identifying the session.</param>
        /// <returns>A collection of exercises selected for the session.</returns>
        [HttpGet("session")]
        public async Task<ActionResult<IEnumerable<AigcExerciseResponseDto>>> GetSessionExercises([FromQuery] string levelId, [FromQuery] int userId)
        {
            _logger.LogInformation("Generating session for user {UserId} at level {LevelId}.", userId, levelId);
            if (string.IsNullOrEmpty(levelId)) return BadRequest("LevelId is required");

            // 1. Check Inventory & Trigger AI if low
            var userAttemptedIds = await _context.UserExerciseAttempts
                .Where(a => a.UserId == userId)
                .Select(a => a.ExerciseId)
                .Distinct()
                .ToListAsync();

            var unattemptedCount = await _context.AigcExercises
                .CountAsync(e => e.LevelId == levelId && e.Status == "APPROVED" && !userAttemptedIds.Contains(e.Id));

            if (unattemptedCount < 20)
            {
                _logger.LogInformation("Unattempted count for level {LevelId} is low ({Count}). Triggering background generation.", levelId, unattemptedCount);
                // Fire and forget generation for next time (or wait a bit)
                // In a production app, this would be a background job.
                _ = Task.Run(async () => await GenerateMoreAsync(levelId));
            }

            // 2. Fetch Session Exercises
            var allLevelExercises = await _context.AigcExercises
                .Where(e => (e.LevelId == levelId || e.LevelId.StartsWith(levelId + "_")) && e.Status == "APPROVED")
                .ToListAsync();

            if (!allLevelExercises.Any()) 
            {
                // If totally empty, generate synchronously once to at least have something
                await GenerateMoreAsync(levelId);
                allLevelExercises = await _context.AigcExercises
                    .Where(e => (e.LevelId == levelId || e.LevelId.StartsWith(levelId + "_")) && e.Status == "APPROVED")
                    .ToListAsync();
            }

            var userAttempts = await _context.UserExerciseAttempts
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.AttemptDate)
                .ToListAsync();

            var failedExerciseIds = userAttempts
                .GroupBy(a => a.ExerciseId)
                .Where(g => !g.First().IsCorrect) 
                .Select(g => g.Key)
                .ToList();

            var attemptedExerciseIds = userAttempts.Select(a => a.ExerciseId).Distinct().ToList();

            var sessionExercises = new List<AigcExercise>();
            var random = new Random();

            // 1. Get up to 5 failed ones
            var failedToAdd = allLevelExercises
                .Where(e => failedExerciseIds.Contains(e.Id))
                .OrderBy(x => random.Next())
                .Take(5)
                .ToList();
            sessionExercises.AddRange(failedToAdd);

            // 2. Get up to 10 totally new ones
            var newToAdd = allLevelExercises
                .Where(e => !attemptedExerciseIds.Contains(e.Id))
                .OrderBy(x => random.Next())
                .Take(10)
                .ToList();
            sessionExercises.AddRange(newToAdd);

            // 3. Fill up to 15 with random ones if short
            if (sessionExercises.Count < 15)
            {
                var needed = 15 - sessionExercises.Count;
                var filler = allLevelExercises
                    .Where(e => !sessionExercises.Select(s => s.Id).Contains(e.Id))
                    .OrderBy(x => random.Next())
                    .Take(needed)
                    .ToList();
                sessionExercises.AddRange(filler);
            }

            sessionExercises = sessionExercises.OrderBy(x => random.Next()).ToList();

            var result = sessionExercises.Select(e => new AigcExerciseResponseDto
            {
                Id = e.Id,
                ExerciseCode = e.ExerciseCode,
                TemplateType = e.TemplateType,
                LevelId = e.LevelId,
                Topics = e.Topics,
                Difficulty = e.Difficulty,
                Status = e.Status,
                JsonSchema = e.JsonSchema
            }).ToList();

            return Ok(result);
        }

        private async Task GenerateMoreAsync(string levelId)
        {
            try 
            {
                var context = await _knowledgeService.GetNextContextAsync(levelId);
                var newExercises = await _aiService.GenerateAigcExercisesAsync(levelId, context.Content, 5);
                
                foreach (var ex in newExercises)
                {
                    ex.SourceMaterial = context.BookName;
                    ex.SourcePage = context.PageNumber;
                    ex.Status = "APPROVED"; // Auto-approve for now or set to BETA
                }

                _context.AigcExercises.AddRange(newExercises);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully generated {Count} more exercises for level {LevelId}.", newExercises.Count, levelId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during background exercise generation for level {LevelId}.", levelId);
                // Log and swallow so the user request doesn't fail
            }
        }

        // POST: api/aigcexercises/attempt
        /// <summary>
        /// Records a user's attempt on a specific AIGC exercise.
        /// </summary>
        /// <param name="dto">The attempt details (correctness, user, exercise).</param>
        /// <returns>An <see cref="IActionResult"/> indicating success.</returns>
        [HttpPost("attempt")]
        public async Task<IActionResult> RecordAttempt([FromBody] AigcExerciseAttemptDto dto)
        {
            _logger.LogInformation("Recording attempt for user {UserId} on exercise {ExerciseId}. Correct: {IsCorrect}.", dto.UserId, dto.ExerciseId, dto.IsCorrect);
            var attempt = new UserExerciseAttempt
            {
                UserId = dto.UserId,
                ExerciseId = dto.ExerciseId,
                IsCorrect = dto.IsCorrect,
                AttemptDate = DateTime.UtcNow
            };

            _context.UserExerciseAttempts.Add(attempt);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
