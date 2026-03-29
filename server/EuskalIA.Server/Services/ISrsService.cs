using EuskalIA.Server.Models;

namespace EuskalIA.Server.Services
{
    public interface ISrsService
    {
        /// <summary>
        /// Updates an SRS node based on a review result.
        /// </summary>
        Task UpdateSrsNodeAsync(int userId, string topic, bool isCorrect);

        /// <summary>
        /// Gets the list of topics due for review for a specific user.
        /// </summary>
        Task<List<UserSrsNode>> GetDueNodesAsync(int userId);

        /// <summary>
        /// Gets the count of pending reviews for a user.
        /// </summary>
        Task<int> GetPendingReviewsCountAsync(int userId);
    }
}
