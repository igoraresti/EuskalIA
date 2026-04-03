using EuskalIA.Server.Data;
using EuskalIA.Server.Models;
using EuskalIA.Server.DTOs.Analytics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EuskalIA.Server.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AnalyticsService> _logger;

        public AnalyticsService(AppDbContext context, ILogger<AnalyticsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<WeaknessDto>> GetUserWeaknessesAsync(int userId, int topN = 3)
        {
            _logger.LogInformation("Calculating top {TopN} weaknesses for user {UserId}.", topN, userId);
            var cutoff = DateTime.UtcNow.AddDays(-30);

            // Fetch recent attempts with their exercises
            var attempts = await _context.UserExerciseAttempts
                .Where(a => a.UserId == userId && a.AttemptDate >= cutoff)
                .Include(a => a.Exercise)
                .ToListAsync();

            if (!attempts.Any())
            {
                _logger.LogInformation("No recent exercise attempts found for user {UserId}.", userId);
                return new List<WeaknessDto>();
            }

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

            var results = topicAttempts
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

            _logger.LogInformation("Identified {Count} weakness topics for user {UserId}.", results.Count, userId);
            return results;
        }
    }
}
