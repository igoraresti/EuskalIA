using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EuskalIA.Server.Services;
using Microsoft.AspNetCore.Authorization;

namespace EuskalIA.Server.Controllers
{
    /// <summary>
    /// Controller for handling gamification features such as achievements and user progress milestones.
    /// </summary>
    [ApiController]
    [Route("api/euskalia/[controller]")]
    public class GamificationController : ControllerBase
    {
        private readonly IGamificationService _gamificationService;
        private readonly ILogger<GamificationController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="GamificationController"/> class.
        /// </summary>
        /// <param name="gamificationService">The gamification service for handling rewards and achievements.</param>
        /// <param name="logger">The controller logger.</param>
        public GamificationController(IGamificationService gamificationService, ILogger<GamificationController> logger)
        {
            _gamificationService = gamificationService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves the list of achievements earned by the user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A collection of earned achievement records.</returns>
        [Authorize]
        [HttpGet("achievements/{userId}")]
        public async Task<IActionResult> GetUserAchievements(int userId)
        {
            var achievements = await _gamificationService.GetUserAchievementsAsync(userId);
            return Ok(achievements);
        }

        /// <summary>
        /// Manually triggers a check for any newly earned achievements for the user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A collection of any achievements newly unlocked during this check.</returns>
        [Authorize]
        [HttpPost("check-achievements/{userId}")]
        public async Task<IActionResult> CheckAchievements(int userId)
        {
            _logger.LogInformation("Manually triggering achievement check for user {UserId}.", userId);
            var newlyEarned = await _gamificationService.CheckAchievementsAsync(userId);
            return Ok(newlyEarned);
        }
    }
}
