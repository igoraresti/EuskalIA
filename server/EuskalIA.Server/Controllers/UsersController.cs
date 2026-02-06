using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EuskalIA.Server.Data;
using EuskalIA.Server.Models;
using EuskalIA.Server.DTOs;
using EuskalIA.Server.Services;

namespace EuskalIA.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEncryptionService _encryptionService;

        public UsersController(AppDbContext context, IEncryptionService encryptionService)
        {
            _context = context;
            _encryptionService = encryptionService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            // Decrypt for response
            return new User 
            {
                Id = user.Id,
                Username = user.Username,
                Nickname = _encryptionService.Decrypt(user.Nickname),
                Email = _encryptionService.Decrypt(user.Email),
                JoinedAt = user.JoinedAt
            };
        }

        [HttpGet("{id}/progress")]
        public async Task<ActionResult<Progress>> GetProgress(int id)
        {
            var user = await _context.Users.Include(u => u.Progress).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                if (id == 1)
                {
                    user = new User 
                    { 
                        Username = "Igor Aresti", 
                        Nickname = _encryptionService.Encrypt("igoraresti"),
                        Email = _encryptionService.Encrypt("igor@euskalia.eus"),
                        Password = _encryptionService.Encrypt("1234"),
                        JoinedAt = DateTime.UtcNow.AddMonths(-2)
                    };
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    return NotFound();
                }
            }

            if (user.Progress == null)
            {
                var progress = new Progress 
                { 
                    UserId = user.Id, 
                    XP = 0, 
                    Level = 1, 
                    Streak = 0, 
                    Txanponak = 100,
                    LastLessonDate = DateTime.Now.AddDays(-1),
                    LastLessonTitle = "Saludos"
                };
                _context.Progresses.Add(progress);
                await _context.SaveChangesAsync();
                user.Progress = progress;
            }
            return user.Progress;
        }

        [HttpPost("{id}/xp")]
        public async Task<IActionResult> AddXP(int id, [FromBody] XPUpdateDto update)
        {
            var progress = await _context.Progresses.FirstOrDefaultAsync(p => p.UserId == id);
            if (progress == null) return NotFound();

            progress.XP += update.XP;
            progress.WeeklyXP += update.XP;
            progress.MonthlyXP += update.XP;
            progress.LastLessonDate = DateTime.Now;
            progress.LastLessonTitle = update.LessonTitle ?? progress.LastLessonTitle;

            // Simple level logic
            progress.Level = (progress.XP / 1000) + 1;
            
            await _context.SaveChangesAsync();
            return Ok(progress);
        }

        [HttpPut("{id}/profile")]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] ProfileUpdateDto profile)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (!string.IsNullOrEmpty(profile.Username)) user.Username = profile.Username;
            if (!string.IsNullOrEmpty(profile.Email)) user.Email = _encryptionService.Encrypt(profile.Email);
            if (!string.IsNullOrEmpty(profile.Nickname)) user.Nickname = _encryptionService.Encrypt(profile.Nickname);
            if (!string.IsNullOrEmpty(profile.Password)) user.Password = _encryptionService.Encrypt(profile.Password);

            await _context.SaveChangesAsync();
            return Ok(new { Message = "Perfil actualizado con éxito" });
        }

        [HttpPost("{id}/request-deletion")]
        public async Task<IActionResult> RequestDeletion(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            var random = new Random();
            var code = random.Next(100000, 999999).ToString();
            user.DeletionCode = code;
            user.CodeExpiration = DateTime.UtcNow.AddMinutes(15);
            await _context.SaveChangesAsync();

            // Simulate sending email by logging to console
            Console.WriteLine("**************************************************");
            Console.WriteLine($"[EMAIL SIMULATION] To: {user.Email}");
            Console.WriteLine($"[EMAIL SIMULATION] Subject: Confirm Account Deletion");
            Console.WriteLine($"[EMAIL SIMULATION] Your verification code is: {code}");
            Console.WriteLine("**************************************************");

            return Ok(new { message = "Verification code generated and sent." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(int id, [FromQuery] string? code)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            if (string.IsNullOrEmpty(code) || user.DeletionCode != code || user.CodeExpiration < DateTime.UtcNow)
            {
                return BadRequest(new { message = "Invalid or expired verification code." });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Account deleted successfully." });
        }
    }
}
