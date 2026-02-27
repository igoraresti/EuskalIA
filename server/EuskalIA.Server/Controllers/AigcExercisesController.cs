using EuskalIA.Server.Data;
using EuskalIA.Server.DTOs;
using EuskalIA.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EuskalIA.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AigcExercisesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AigcExercisesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/aigcexercises?levelId=A1
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
        [HttpGet("session")]
        public async Task<ActionResult<IEnumerable<AigcExerciseResponseDto>>> GetSessionExercises([FromQuery] string levelId, [FromQuery] int userId)
        {
            if (string.IsNullOrEmpty(levelId)) return BadRequest("LevelId is required");

            // Use StartsWith so that "A1" matches both "A1" and "A1_UNIT_1" etc.
            var allLevelExercises = await _context.AigcExercises
                .Where(e => (e.LevelId == levelId || e.LevelId.StartsWith(levelId + "_")) && e.Status == "APPROVED")
                .ToListAsync();

            if (!allLevelExercises.Any()) return Ok(new List<AigcExerciseResponseDto>());

            var userAttempts = await _context.UserExerciseAttempts
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.AttemptDate)
                .ToListAsync();

            var failedExerciseIds = userAttempts
                .GroupBy(a => a.ExerciseId)
                .Where(g => !g.First().IsCorrect) // Latest attempt was a fail
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

            // Shuffle the final list
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

        // POST: api/aigcexercises/attempt
        [HttpPost("attempt")]
        public async Task<IActionResult> RecordAttempt([FromBody] AigcExerciseAttemptDto dto)
        {
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
