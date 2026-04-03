using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EuskalIA.Server.Data;
using EuskalIA.Server.Models;
using EuskalIA.Server.DTOs.Admin;
using EuskalIA.Server.DTOs.Common;
using Microsoft.AspNetCore.Authorization;
using EuskalIA.Server.Services.Encryption;
using System.Text.Json;

namespace EuskalIA.Server.Controllers
{
    /// <summary>
    /// Controller for administrative tasks, including user management and AIGC exercise moderation.
    /// restricted to users with the "Admin" role.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/euskalia/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEncryptionService _encryptionService;
        private readonly ILogger<AdminController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminController"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="encryptionService">The encryption service for handling sensitive user data.</param>
        /// <param name="logger">The controller logger.</param>
        public AdminController(AppDbContext context, IEncryptionService encryptionService, ILogger<AdminController> logger)
        {
            _context = context;
            _encryptionService = encryptionService;
            _logger = logger;
        }

        // ── Users ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Retrieves a paginated and filtered list of users for administration.
        /// </summary>
        /// <param name="filter">The filtering and pagination parameters.</param>
        /// <returns>A paginated list of <see cref="AdminUserDto"/> objects.</returns>
        [HttpGet("users")]
        public async Task<ActionResult<PaginatedList<AdminUserDto>>> GetUsers([FromQuery] AdminUserFilterDto filter)
        {
            var query = _context.Users
                .Include(u => u.Progress)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.ToLower();
                query = query.Where(u => u.Username.ToLower().Contains(search));
            }

            if (filter.IsActive.HasValue)
                query = query.Where(u => u.IsActive == filter.IsActive.Value);

            if (filter.JoinedFrom.HasValue)
                query = query.Where(u => u.JoinedAt >= filter.JoinedFrom.Value);

            if (filter.JoinedTo.HasValue)
                query = query.Where(u => u.JoinedAt <= filter.JoinedTo.Value);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(u => u.JoinedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(u => new AdminUserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = _encryptionService.Decrypt(u.Email),
                    Role = u.Role,
                    IsActive = u.IsActive,
                    IsVerified = u.IsVerified,
                    JoinedAt = u.JoinedAt,
                    XP = u.Progress != null ? u.Progress.XP : 0
                })
                .ToListAsync();

            return Ok(new PaginatedList<AdminUserDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = filter.Page,
                PageSize = filter.PageSize
            });
        }

        /// <summary>
        /// Retrieves global statistics for the admin dashboard, including user counts and AI health.
        /// </summary>
        /// <returns>An <see cref="ActionResult{AdminStatsDto}"/> containing dashboard metrics.</returns>
        [HttpGet("stats")]
        public async Task<ActionResult<AdminStatsDto>> GetStats()
        {
            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
            var today = DateTime.UtcNow.Date;
            var registrationsToday = await _context.Users.CountAsync(u => u.JoinedAt >= today);

            // AI Health
            var lastLogs = await _context.AigcLogs
                .OrderByDescending(l => l.Timestamp)
                .Take(5)
                .ToListAsync();

            var hasErrors = lastLogs.Any(l => l.Status != "SUCCESS");

            return Ok(new 
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                RegistrationsToday = registrationsToday,
                AiHealth = new {
                    Status = hasErrors ? "WARNING" : "HEALTHY",
                    LastLogs = lastLogs
                }
            });
        }

        /// <summary>
        /// Toggles the active status of a user.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <returns>An <see cref="IActionResult"/> with the new active status.</returns>
        [HttpPut("users/{id}/toggle-active")]
        public async Task<IActionResult> ToggleUserActive(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                _logger.LogWarning("Admin tried to toggle active status for non-existent user {UserId}.", id);
                return NotFound();
            }

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Admin toggled active status for user {Username} (ID: {UserId}) to {IsActive}.", user.Username, id, user.IsActive);

            return Ok(new { isActive = user.IsActive });
        }

        // ── Exercises ─────────────────────────────────────────────────────────

        /// <summary>
        /// Retrieves a paginated list of AIGC exercises with filtering and sorting options.
        /// Includes completion statistics (Correct/Wrong attempts).
        /// </summary>
        /// <param name="levelId">Filter by level (optional).</param>
        /// <param name="topic">Filter by topic (optional).</param>
        /// <param name="status">Filter by moderation status (optional).</param>
        /// <param name="search">Search term for exercise code or content (optional).</param>
        /// <param name="sortBy">The field to sort by (default: exerciseCode).</param>
        /// <param name="sortDir">The sort direction (asc/desc).</param>
        /// <param name="page">The page number.</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>A paginated list of exercises with extra stats.</returns>
        [HttpGet("exercises")]
        public async Task<IActionResult> GetExercises(
            [FromQuery] string? levelId,
            [FromQuery] string? topic,
            [FromQuery] string? status,
            [FromQuery] string? search,
            [FromQuery] string? sortBy  = "exerciseCode",
            [FromQuery] string? sortDir = "asc",
            [FromQuery] int page     = 1,
            [FromQuery] int pageSize = 20)
        {
            var baseQuery = _context.AigcExercises.AsQueryable();

            if (!string.IsNullOrWhiteSpace(levelId))
                baseQuery = baseQuery.Where(e => e.LevelId == levelId);
            if (!string.IsNullOrWhiteSpace(topic))
                baseQuery = baseQuery.Where(e => e.Topics == topic);
            if (!string.IsNullOrWhiteSpace(status))
                baseQuery = baseQuery.Where(e => e.Status == status);
            if (!string.IsNullOrWhiteSpace(search))
                baseQuery = baseQuery.Where(e => e.ExerciseCode.Contains(search) || e.JsonSchema.Contains(search));

            // Project with attempt counts in a single query
            var projected = baseQuery.Select(e => new
            {
                e.Id,
                e.ExerciseCode,
                e.TemplateType,
                e.LevelId,
                e.Topics,
                e.Difficulty,
                e.Status,
                e.JsonSchema,
                e.CreatedAt,
                Correct = _context.UserExerciseAttempts.Count(a => a.ExerciseId == e.Id && a.IsCorrect),
                Wrong   = _context.UserExerciseAttempts.Count(a => a.ExerciseId == e.Id && !a.IsCorrect),
            });

            var total = await projected.CountAsync();

            var sorted = (sortBy?.ToLower(), sortDir?.ToLower()) switch
            {
                ("levelid",      "desc") => projected.OrderByDescending(e => e.LevelId),
                ("levelid",      _)      => projected.OrderBy(e => e.LevelId),
                ("topics",       "desc") => projected.OrderByDescending(e => e.Topics),
                ("topics",       _)      => projected.OrderBy(e => e.Topics),
                ("templatetype", "desc") => projected.OrderByDescending(e => e.TemplateType),
                ("templatetype", _)      => projected.OrderBy(e => e.TemplateType),
                ("status",       "desc") => projected.OrderByDescending(e => e.Status),
                ("status",       _)      => projected.OrderBy(e => e.Status),
                ("correct",      "desc") => projected.OrderByDescending(e => e.Correct),
                ("correct",      _)      => projected.OrderBy(e => e.Correct),
                ("wrong",        "desc") => projected.OrderByDescending(e => e.Wrong),
                ("wrong",        _)      => projected.OrderBy(e => e.Wrong),
                ("createdat",    "desc") => projected.OrderByDescending(e => e.CreatedAt),
                ("createdat",    _)      => projected.OrderBy(e => e.CreatedAt),
                (_,              "desc") => projected.OrderByDescending(e => e.ExerciseCode),
                _                        => projected.OrderBy(e => e.ExerciseCode),
            };

            var items = await sorted
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { total, page, pageSize, items });
        }

        /// <summary>
        /// Retrieves detailed statistics for a specific exercise, including attempt counts and unique users.
        /// </summary>
        /// <param name="id">The unique identifier of the exercise.</param>
        /// <returns>An <see cref="IActionResult"/> with exercise stats.</returns>
        [HttpGet("exercises/{id}/stats")]
        public async Task<IActionResult> GetExerciseStats(Guid id)
        {
            var exercise = await _context.AigcExercises.FindAsync(id);
            if (exercise == null) return NotFound();

            var attempts = await _context.UserExerciseAttempts
                .Where(a => a.ExerciseId == id)
                .ToListAsync();

            return Ok(new
            {
                exerciseCode = exercise.ExerciseCode,
                correct      = attempts.Count(a => a.IsCorrect),
                wrong        = attempts.Count(a => !a.IsCorrect),
                total        = attempts.Count,
                uniqueUsers  = attempts.Select(a => a.UserId).Distinct().Count()
            });
        }

        /// <summary>
        /// Deletes an exercise and all its associated user attempts.
        /// </summary>
        /// <param name="id">The unique identifier of the exercise.</param>
        /// <returns>An <see cref="IActionResult"/> indicating success.</returns>
        [HttpDelete("exercises/{id}")]
        public async Task<IActionResult> DeleteExercise(Guid id)
        {
            var exercise = await _context.AigcExercises.FindAsync(id);
            if (exercise == null)
            {
                _logger.LogWarning("Admin tried to delete non-existent exercise {ExerciseId}.", id);
                return NotFound();
            }

            var attempts = _context.UserExerciseAttempts.Where(a => a.ExerciseId == id);
            _context.UserExerciseAttempts.RemoveRange(attempts);
            _context.AigcExercises.Remove(exercise);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Admin deleted exercise {ExerciseCode} (ID: {ExerciseId}).", exercise.ExerciseCode, id);

            return Ok(new { deleted = exercise.ExerciseCode });
        }


        /// <summary>
        /// Updates the moderation status of multiple exercises at once.
        /// </summary>
        /// <param name="req">The request containing exercise IDs and the new status.</param>
        /// <returns>An <see cref="IActionResult"/> with the count of updated exercises.</returns>
        [HttpPatch("exercises/bulk-status")]
        public async Task<IActionResult> BulkUpdateStatus([FromBody] BulkStatusRequest req)
        {
            var validStatuses = new[] { "BETA", "APPROVED", "REJECTED" };
            if (!validStatuses.Contains(req.Status))
            {
                _logger.LogWarning("Admin tried to update exercises to invalid status {Status}.", req.Status);
                return BadRequest($"Status must be one of: {string.Join(", ", validStatuses)}");
            }

            var exercises = await _context.AigcExercises
                .Where(e => req.Ids.Contains(e.Id))
                .ToListAsync();

            foreach (var ex in exercises)
                ex.Status = req.Status;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Admin updated status to {Status} for {Count} exercises.", req.Status, exercises.Count);

            return Ok(new { updated = exercises.Count, status = req.Status });
        }

        /// <summary>
        /// Retrieves a paginated list of AI generation logs for monitoring.
        /// </summary>
        /// <param name="page">The page number.</param>
        /// <param name="pageSize">The items per page.</param>
        /// <returns>A paginated list of <see cref="AigcLog"/> entries.</returns>
        [HttpGet("ai-logs")]
        public async Task<IActionResult> GetAiLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var query = _context.AigcLogs.AsQueryable();
            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { total, page, pageSize, items });
        }


        // ── Import ────────────────────────────────────────────────────────────


        /// <summary>
        /// Handles the import of exercises from an external source (e.g., bulk AI generation).
        /// Performs duplicate detection based on text similarity before importing.
        /// </summary>
        /// <param name="req">The import request containing exercises and configuration.</param>
        /// <returns>An <see cref="IActionResult"/> with a preview or the import results.</returns>
        [HttpPost("exercises/import")]
        public async Task<IActionResult> ImportExercises([FromBody] ImportRequest req)
        {
            var topics = req.Exercises.Select(e => e.Topics.ToLower()).Distinct().ToList();
            var levels = req.Exercises.Select(e => e.LevelId.ToUpper()).Distinct().ToList();

            var existing = await _context.AigcExercises
                .Where(e => levels.Contains(e.LevelId) && topics.Contains(e.Topics))
                .Select(e => new { e.ExerciseCode, e.Topics, e.LevelId, e.JsonSchema })
                .ToListAsync();

            var existingMeta = existing.Select(e => (
                code:  e.ExerciseCode,
                topic: e.Topics,
                level: e.LevelId,
                es:    ExtractEsText(e.JsonSchema) ?? "",
                en:    ExtractEnText(e.JsonSchema) ?? ""
            )).ToList();

            var results   = new List<object>();
            var toInsert  = new List<ImportExerciseItem>();

            foreach (var item in req.Exercises)
            {
                var inEs = ExtractEsText(item.JsonSchema) ?? "";
                var inEn = ExtractEnText(item.JsonSchema) ?? "";

                double bestSim    = 0;
                string? matchCode = null;

                foreach (var ex in existingMeta.Where(x =>
                    x.level.Equals(item.LevelId, StringComparison.OrdinalIgnoreCase) &&
                    x.topic.Equals(item.Topics,  StringComparison.OrdinalIgnoreCase)))
                {
                    var avg = (Similarity(inEs, ex.es) + Similarity(inEn, ex.en)) / 2.0;
                    if (avg > bestSim) { bestSim = avg; matchCode = ex.code; }
                }

                var isDup = bestSim >= req.Threshold;

                results.Add(new
                {
                    item.LevelId,
                    item.Topics,
                    item.TemplateType,
                    QuestionEs    = inEs,
                    IsDuplicate   = isDup,
                    SimilarityPct = Math.Round(bestSim * 100, 1),
                    MatchedCode   = isDup ? matchCode : null
                });

                if (req.Confirm && !isDup) toInsert.Add(item);
            }

            if (req.Confirm && toInsert.Count > 0)
            {
                _logger.LogInformation("Admin confirmed import of {Count} exercises.", toInsert.Count);
                var counters = new Dictionary<string, int>();
                foreach (var item in toInsert)
                {
                    var key = $"{item.LevelId.ToLower()}_{item.Topics.ToLower()}";
                    if (!counters.ContainsKey(key))
                        counters[key] = await _context.AigcExercises
                            .CountAsync(e => e.LevelId == item.LevelId && e.Topics == item.Topics);

                    _context.AigcExercises.Add(new AigcExercise
                    {
                        ExerciseCode = $"{item.LevelId.ToLower()}_{item.Topics.ToLower()}_{counters[key]++}",
                        TemplateType = item.TemplateType,
                        LevelId      = item.LevelId.ToUpper(),
                        Topics       = item.Topics.ToLower(),
                        Difficulty   = item.Difficulty,
                        JsonSchema   = item.JsonSchema,
                        Status       = "BETA",
                        CreatedAt    = DateTime.UtcNow
                    });
                }
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                preview  = results,
                imported = req.Confirm ? toInsert.Count : (int?)null
            });
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static string? ExtractEsText(string json)
        {
            try
            {
                var root = JsonDocument.Parse(json).RootElement;
                if (root.TryGetProperty("question", out var q) && q.TryGetProperty("es", out var v)) return v.GetString()?.ToLower().Trim();
                if (root.TryGetProperty("promptLocal", out var p) && p.TryGetProperty("es", out var pv)) return pv.GetString()?.ToLower().Trim();
            }
            catch { }
            return null;
        }

        private static string? ExtractEnText(string json)
        {
            try
            {
                var root = JsonDocument.Parse(json).RootElement;
                if (root.TryGetProperty("question", out var q) && q.TryGetProperty("en", out var v)) return v.GetString()?.ToLower().Trim();
                if (root.TryGetProperty("promptLocal", out var p) && p.TryGetProperty("en", out var pv)) return pv.GetString()?.ToLower().Trim();
            }
            catch { }
            return null;
        }

        private static double Similarity(string a, string b)
        {
            if (string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b)) return 1.0;
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return 0.0;
            return 1.0 - (double)Levenshtein(a, b) / Math.Max(a.Length, b.Length);
        }

        private static int Levenshtein(string s, string t)
        {
            int n = s.Length, m = t.Length;
            var d = new int[n + 1, m + 1];
            for (int i = 0; i <= n; i++) d[i, 0] = i;
            for (int j = 0; j <= m; j++) d[0, j] = j;
            for (int i = 1; i <= n; i++)
                for (int j = 1; j <= m; j++)
                    d[i, j] = s[i - 1] == t[j - 1]
                        ? d[i - 1, j - 1]
                        : 1 + Math.Min(d[i - 1, j], Math.Min(d[i, j - 1], d[i - 1, j - 1]));
            return d[n, m];
        }
    }
}
