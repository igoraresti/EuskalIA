using EuskalIA.Server.Models;

namespace EuskalIA.Server.Services
{
    public interface IGamificationService
    {
        /// <summary>
        /// Updates the user's streak based on the current activity date.
        /// </summary>
        Task<int> UpdateStreakAsync(int userId);

        /// <summary>
        /// Evaluates all available achievements for a user and unlocks new ones if criteria are met.
        /// </summary>
        Task<List<Achievement>> CheckAchievementsAsync(int userId);

        /// <summary>
        /// Gets all achievements, marking which ones the user has already earned.
        /// </summary>
        Task<List<UserAchievementDto>> GetUserAchievementsAsync(int userId);
    }

    public class UserAchievementDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int TargetValue { get; set; }
        public bool IsEarned { get; set; }
        public DateTime? EarnedAt { get; set; }
    }
}
