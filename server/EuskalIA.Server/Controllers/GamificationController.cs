using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using EuskalIA.Server.Services;
using Microsoft.AspNetCore.Authorization;

namespace EuskalIA.Server.Controllers
{
    [ApiController]
    [Route("api/euskalia/[controller]")]
    public class GamificationController : ControllerBase
    {
        private readonly IGamificationService _gamificationService;
        private readonly ILogger<GamificationController> _logger;

        public GamificationController(IGamificationService gamificationService, ILogger<GamificationController> logger)
        {
            _gamificationService = gamificationService;
            _logger = logger;
        }

        [Authorize]
        [HttpGet("achievements/{userId}")]
        public async Task<IActionResult> GetUserAchievements(int userId)
        {
            var achievements = await _gamificationService.GetUserAchievementsAsync(userId);
            return Ok(achievements);
        }

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
