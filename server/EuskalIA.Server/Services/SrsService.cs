using EuskalIA.Server.Data;
using EuskalIA.Server.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EuskalIA.Server.Services
{
    public class SrsService : ISrsService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SrsService> _logger;
        private const float DefaultEaseFactor = 2.5f;
        private const float MinEaseFactor = 1.3f;

        public SrsService(AppDbContext context, ILogger<SrsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task UpdateSrsNodeAsync(int userId, string topic, bool isCorrect)
        {
            _logger.LogInformation("Updating SRS node for user {UserId}, topic {Topic}, success: {IsCorrect}.", userId, topic, isCorrect);
            var node = await _context.UserSrsNodes
                .FirstOrDefaultAsync(n => n.UserId == userId && n.ConceptId == topic);

            if (node == null)
            {
                // New concept learned
                node = new UserSrsNode
                {
                    UserId = userId,
                    ConceptId = topic,
                    MasteryLevel = isCorrect ? 1.0f : 0.0f,
                    RiskFactor = DefaultEaseFactor, // Using RiskFactor as EaseFactor (EF)
                    LastReviewDate = DateTime.UtcNow,
                    NextReviewDate = isCorrect ? DateTime.UtcNow.AddDays(1) : DateTime.UtcNow
                };
                _context.UserSrsNodes.Add(node);
            }
            else
            {
                // Existing node, apply SM-2 logic
                float ef = node.RiskFactor > 0 ? node.RiskFactor : DefaultEaseFactor;
                int interval = (int)((node.NextReviewDate ?? DateTime.UtcNow) - (node.LastReviewDate ?? DateTime.UtcNow.AddDays(-1))).TotalDays;
                if (interval < 1) interval = 1;

                if (isCorrect)
                {
                    // Success logic
                    if (interval == 1) interval = 6;
                    else interval = (int)Math.Round(interval * ef);

                    // Slightly increase Ease Factor for correct answers
                    ef = Math.Min(2.5f, ef + 0.1f);
                }
                else
                {
                    // Fail logic
                    interval = 1;
                    // Slightly decrease Ease Factor for mistakes
                    ef = Math.Max(MinEaseFactor, ef - 0.2f);
                }

                node.LastReviewDate = DateTime.UtcNow;
                node.NextReviewDate = DateTime.UtcNow.AddDays(interval);
                node.RiskFactor = ef;
                node.MasteryLevel = isCorrect ? Math.Min(5.0f, node.MasteryLevel + 0.5f) : Math.Max(0.0f, node.MasteryLevel - 1.0f);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("SRS node for user {UserId}, topic {Topic} updated. Next review: {NextReview}.", userId, topic, node.NextReviewDate);
        }

        public async Task<List<UserSrsNode>> GetDueNodesAsync(int userId)
        {
            return await _context.UserSrsNodes
                .Where(n => n.UserId == userId && (n.NextReviewDate == null || n.NextReviewDate <= DateTime.UtcNow))
                .ToListAsync();
        }

        public async Task<int> GetPendingReviewsCountAsync(int userId)
        {
            return await _context.UserSrsNodes
                .CountAsync(n => n.UserId == userId && (n.NextReviewDate == null || n.NextReviewDate <= DateTime.UtcNow));
        }
    }
}
