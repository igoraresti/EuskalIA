using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EuskalIA.Server.Data;
using EuskalIA.Server.Models;

using Microsoft.AspNetCore.Authorization;

namespace EuskalIA.Server.Controllers
{
    /// <summary>
    /// Controller for managing and retrieving user rankings and leaderboards.
    /// Supports daily, weekly, monthly, and global (all-time) scopes.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/euskalia/[controller]")]
    public class LeaderboardController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<LeaderboardController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LeaderboardController"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="logger">The controller logger.</param>
        public LeaderboardController(AppDbContext context, ILogger<LeaderboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves the top 10 players for a given timeframe (all, week, or month).
        /// </summary>
        /// <param name="period">The timeframe to filter by ("all", "week", or "month"). Defaults to "all".</param>
        /// <returns>A collection of the top 10 users with their XP and level.</returns>
        [HttpGet("world")]
        public async Task<ActionResult<IEnumerable<dynamic>>> GetWorldLeaderboard(string period = "all")
        {
            _logger.LogInformation("Fetching world leaderboard for period {Period}.", period);
            var query = _context.Progresses
                .Include(p => p.User)
                .Where(p => p.User != null && p.User.IsActive)
                .AsQueryable();

            query = period switch
            {
                "week" => query.OrderByDescending(p => p.WeeklyXP),
                "month" => query.OrderByDescending(p => p.MonthlyXP),
                _ => query.OrderByDescending(p => p.XP)
            };

            var top10 = await query
                .Take(10)
                .Select(p => new
                {
                    p.UserId,
                    Username = p.User != null ? p.User.Username : "Alumno",
                    XP = period == "week" ? p.WeeklyXP : (period == "month" ? p.MonthlyXP : p.XP),
                    p.Level
                })
                .ToListAsync();

            return Ok(top10);
        }

        /// <summary>
        /// Retrieves a relative leaderboard showing the user and their immediate neighbors in the ranking.
        /// </summary>
        /// <param name="userId">The unique identifier of the user (to center the ranking around them).</param>
        /// <param name="period">The timeframe to filter by ("all", "week", or "month"). Defaults to "all".</param>
        /// <returns>A slice of the leaderboard containing the user and neighboring participants.</returns>
        [HttpGet("me/{userId}")]
        public async Task<ActionResult<IEnumerable<dynamic>>> GetUserLeaderboard(int userId, string period = "all")
        {
            _logger.LogInformation("Fetching relative leaderboard for user {UserId} (Period: {Period}).", userId, period);
            var allRankings = await _context.Progresses
                .Include(p => p.User)
                .Where(p => p.User != null && p.User.IsActive)
                .Select(p => new
                {
                    p.UserId,
                    Username = p.User != null ? p.User.Username : "Alumno",
                    XP = period == "week" ? p.WeeklyXP : (period == "month" ? p.MonthlyXP : p.XP),
                    p.Level
                })
                .ToListAsync();

            var sortedRankings = period switch
            {
                "week" => allRankings.OrderByDescending(r => r.XP).ToList(),
                "month" => allRankings.OrderByDescending(r => r.XP).ToList(),
                _ => allRankings.OrderByDescending(r => r.XP).ToList()
            };

            var userIndex = sortedRankings.FindIndex(r => r.UserId == userId);
            if (userIndex == -1) return NotFound();

            var startIndex = Math.Max(0, userIndex - 5);
            var count = Math.Min(11, sortedRankings.Count - startIndex);

            var relativeRankings = sortedRankings.GetRange(startIndex, count)
                .Select((r, index) => new
                {
                    r.UserId,
                    r.Username,
                    r.XP,
                    r.Level,
                    Rank = startIndex + index + 1
                });

            return Ok(relativeRankings);
        }

        /// <summary>
        /// Retrieves the absolute global rank of a specific user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>An object containing the user's rank and the total number of active users.</returns>
        [HttpGet("rank/{userId}")]
        public async Task<ActionResult<object>> GetUserGlobalRank(int userId)
        {
            var allXps = await _context.Progresses
                .Where(p => p.User != null && p.User.IsActive)
                .OrderByDescending(p => p.XP)
                .Select(p => p.UserId)
                .ToListAsync();

            var rank = allXps.IndexOf(userId) + 1;
            if (rank == 0) return NotFound();

            return Ok(new { rank, total = allXps.Count });
        }
    }
}
