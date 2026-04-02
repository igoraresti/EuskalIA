using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EuskalIA.Server.Services;

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

        public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
        {
            _analyticsService = analyticsService;
            _logger = logger;
        }

        /// <summary>
        /// Returns the top N topics the user struggles with most in the last 30 days.
        /// </summary>
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
