using EuskalIA.Server.Models;
using EuskalIA.Server.DTOs.Gamification;

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
}
