using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EuskalIA.Server.Services;
using EuskalIA.Server.DTOs.Analytics;

namespace EuskalIA.Server.Controllers
{
    /// <summary>
    /// API endpoints for user learning analytics: weaknesses, trends, and performance insights.
    /// </summary>
    [ApiController]
    [Route("api/euskalia/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogger<AnalyticsController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalyticsController"/> class.
        /// </summary>
        /// <param name="analyticsService">The analytics service for performance analysis.</param>
        /// <param name="logger">The controller logger.</param>
        public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
        {
            _analyticsService = analyticsService;
            _logger = logger;
        }

        /// <summary>
        /// Returns the top N topics the user struggles with most in the last 30 days.
        /// Useful for personalized learning recommendations.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="topN">The number of top weaknesses to return (default is 3).</param>
        /// <returns>A collection of topics and their failure rates.</returns>
        [Authorize]
        [HttpGet("weaknesses/{userId}")]
        public async Task<IActionResult> GetWeaknesses(int userId, [FromQuery] int topN = 3)
        {
            _logger.LogInformation("Fetching top {TopN} weaknesses for user {UserId}.", topN, userId);
            var weaknesses = await _analyticsService.GetUserWeaknessesAsync(userId, topN);
            return Ok(weaknesses);
        }
    }
}
