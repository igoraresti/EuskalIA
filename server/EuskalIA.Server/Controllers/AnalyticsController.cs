using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        /// <summary>
        /// Returns the top N topics the user struggles with most in the last 30 days.
        /// </summary>
        [Authorize]
        [HttpGet("weaknesses/{userId}")]
        public async Task<IActionResult> GetWeaknesses(int userId, [FromQuery] int topN = 3)
        {
            var weaknesses = await _analyticsService.GetUserWeaknessesAsync(userId, topN);
            return Ok(weaknesses);
        }
    }
}
