using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EuskalIA.Server.Data;
using EuskalIA.Server.Models;

namespace EuskalIA.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaderboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LeaderboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("world")]
        public async Task<ActionResult<IEnumerable<dynamic>>> GetWorldLeaderboard(string period = "all")
        {
            var query = _context.Progresses
                .Include(p => p.User)
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

        [HttpGet("me/{userId}")]
        public async Task<ActionResult<IEnumerable<dynamic>>> GetUserLeaderboard(int userId, string period = "all")
        {
            var allRankings = await _context.Progresses
                .Include(p => p.User)
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
    }
}
