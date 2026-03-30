using EuskalIA.Server.Data;
using EuskalIA.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace EuskalIA.Server.Services
{
    /// <summary>
    /// Represents a user's weakness in a particular topic.
    /// </summary>
    public class WeaknessDto
    {
        public string Topic { get; set; } = string.Empty;
        public int FailureCount { get; set; }
        public int TotalAttempts { get; set; }
        public double FailureRate => TotalAttempts > 0 ? (double)FailureCount / TotalAttempts : 0;
    }

    public interface IAnalyticsService
    {
        /// <summary>
        /// Returns the top weakest topics for a user based on failed exercise attempts in the last 30 days.
        /// </summary>
        Task<List<WeaknessDto>> GetUserWeaknessesAsync(int userId, int topN = 3);
    }

    public class AnalyticsService : IAnalyticsService
    {
        private readonly AppDbContext _context;

        public AnalyticsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<WeaknessDto>> GetUserWeaknessesAsync(int userId, int topN = 3)
        {
            var cutoff = DateTime.UtcNow.AddDays(-30);

            // Fetch recent attempts with their exercises
            var attempts = await _context.UserExerciseAttempts
                .Where(a => a.UserId == userId && a.AttemptDate >= cutoff)
                .Include(a => a.Exercise)
                .ToListAsync();

            if (!attempts.Any())
                return new List<WeaknessDto>();

            // Expand topics (each exercise stores topics as comma-separated string)
            var topicAttempts = new Dictionary<string, (int failures, int total)>(StringComparer.OrdinalIgnoreCase);

            foreach (var attempt in attempts)
            {
                var topics = attempt.Exercise?.Topics
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    ?? Array.Empty<string>();

                // If no topic tagged, group under "Sin Clasificar"
                if (!topics.Any())
                    topics = new[] { "Sin Clasificar" };

                foreach (var topic in topics)
                {
                    if (!topicAttempts.ContainsKey(topic))
                        topicAttempts[topic] = (0, 0);

                    var current = topicAttempts[topic];
                    topicAttempts[topic] = (
                        current.failures + (attempt.IsCorrect ? 0 : 1),
                        current.total + 1
                    );
                }
            }

            return topicAttempts
                .Where(kv => kv.Value.failures > 0) // Only topics with at least one failure
                .OrderByDescending(kv => kv.Value.failures)
                .Take(topN)
                .Select(kv => new WeaknessDto
                {
                    Topic = kv.Key,
                    FailureCount = kv.Value.failures,
                    TotalAttempts = kv.Value.total
                })
                .ToList();
        }
    }
}
