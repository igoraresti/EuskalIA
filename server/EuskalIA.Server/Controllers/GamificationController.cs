using Microsoft.AspNetCore.Mvc;
using EuskalIA.Server.Services;
using Microsoft.AspNetCore.Authorization;

namespace EuskalIA.Server.Controllers
{
    [ApiController]
    [Route("api/euskalia/[controller]")]
    public class GamificationController : ControllerBase
    {
        private readonly IGamificationService _gamificationService;

        public GamificationController(IGamificationService gamificationService)
        {
            _gamificationService = gamificationService;
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
            var newlyEarned = await _gamificationService.CheckAchievementsAsync(userId);
            return Ok(newlyEarned);
        }
    }
}
