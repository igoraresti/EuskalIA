using EuskalIA.Server.Data;
using EuskalIA.Server.Models;
using EuskalIA.Server.DTOs.Gamification;
using Microsoft.EntityFrameworkCore;

namespace EuskalIA.Server.Services
{
    public class GamificationService : IGamificationService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<GamificationService> _logger;

        public GamificationService(AppDbContext context, ILogger<GamificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> UpdateStreakAsync(int userId)
        {
            _logger.LogInformation("Updating streak for user {UserId}.", userId);
            var progress = await _context.Progresses.FirstOrDefaultAsync(p => p.UserId == userId);
            if (progress == null)
            {
                _logger.LogWarning("Progress not found for user {UserId} while updating streak.", userId);
                return 0;
            }

            var today = DateTime.UtcNow.Date;
            var lastLessonDate = progress.LastLessonDate.Date;

            if (lastLessonDate == today)
            {
                // Already practiced today, keep streak
                return progress.Streak;
            }
            
            if (lastLessonDate == today.AddDays(-1))
            {
                // Practiced yesterday, increment streak
                progress.Streak++;
            }
            else
            {
                // Missed a day or more, reset streak to 1
                progress.Streak = 1;
            }

            progress.LastLessonDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _logger.LogInformation("User {UserId} streak updated to {Streak}.", userId, progress.Streak);
            return progress.Streak;
        }

        public async Task<List<Achievement>> CheckAchievementsAsync(int userId)
        {
            _logger.LogInformation("Checking achievements for user {UserId}.", userId);
            var progress = await _context.Progresses.Include(p => p.User).FirstOrDefaultAsync(p => p.UserId == userId);
            if (progress == null)
            {
                _logger.LogWarning("Progress not found for user {UserId} while checking achievements.", userId);
                return new List<Achievement>();
            }

            var earnedAchievementIds = await _context.UserAchievements
                .Where(ua => ua.UserId == userId)
                .Select(ua => ua.AchievementId)
                .ToListAsync();

            var potentialAchievements = await _context.Achievements
                .Where(a => !earnedAchievementIds.Contains(a.Id))
                .ToListAsync();

            var newlyEarned = new List<Achievement>();

            foreach (var achievement in potentialAchievements)
            {
                bool isEarned = false;

                switch (achievement.Category)
                {
                    case "XP":
                        isEarned = progress.XP >= achievement.TargetValue;
                        break;
                    case "STREAK":
                        isEarned = progress.Streak >= achievement.TargetValue;
                        break;
                    case "LESSONS":
                        var completedCount = await _context.LessonProgresses.CountAsync(lp => lp.UserId == userId);
                        isEarned = completedCount >= achievement.TargetValue;
                        break;
                }

                if (isEarned)
                {
                    _context.UserAchievements.Add(new UserAchievement
                    {
                        UserId = userId,
                        AchievementId = achievement.Id,
                        EarnedAt = DateTime.UtcNow
                    });
                    newlyEarned.Add(achievement);
                }
            }

            if (newlyEarned.Any())
            {
                await _context.SaveChangesAsync();
                foreach (var ach in newlyEarned)
                {
                    _logger.LogInformation("User {UserId} earned achievement {AchievementCode}.", userId, ach.Code);
                }
            }

            return newlyEarned;
        }

        public async Task<List<UserAchievementDto>> GetUserAchievementsAsync(int userId)
        {
            var allAchievements = await _context.Achievements.ToListAsync();
            var earnedMap = await _context.UserAchievements
                .Where(ua => ua.UserId == userId)
                .ToDictionaryAsync(ua => ua.AchievementId, ua => ua.EarnedAt);

            return allAchievements.Select(a => new UserAchievementDto
            {
                Id = a.Id,
                Code = a.Code,
                Name = a.Name,
                Description = a.Description,
                Icon = a.Icon,
                Category = a.Category,
                TargetValue = a.TargetValue,
                IsEarned = earnedMap.ContainsKey(a.Id),
                EarnedAt = earnedMap.ContainsKey(a.Id) ? earnedMap[a.Id] : (DateTime?)null
            }).ToList();
        }
    }
}
