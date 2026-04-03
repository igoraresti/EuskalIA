using EuskalIA.Server.DTOs.Analytics;

namespace EuskalIA.Server.Services
{
    public interface IAnalyticsService
    {
        /// <summary>
        /// Returns the top weakest topics for a user based on failed exercise attempts in the last 30 days.
        /// </summary>
        Task<List<WeaknessDto>> GetUserWeaknessesAsync(int userId, int topN = 3);
    }
}
