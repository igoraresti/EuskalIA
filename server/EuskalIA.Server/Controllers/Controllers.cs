using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EuskalIA.Server.Data;
using EuskalIA.Server.Models;
using EuskalIA.Server.Services;

namespace EuskalIA.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LessonsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAIService _aiService;
        private readonly IEncryptionService _encryptionService;

        public LessonsController(AppDbContext context, IAIService aiService, IEncryptionService encryptionService)
        {
            _context = context;
            _aiService = aiService;
            _encryptionService = encryptionService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Lesson>>> GetLessons()
        {
            // Seed some data if empty
            if (!await _context.Lessons.AnyAsync())
            {
                var lessons = new List<Lesson>
                {
                    new Lesson { Title = "Saludos", Topic = "Saludos", Level = 1 },
                    new Lesson { Title = "La Comida", Topic = "Comida", Level = 1 },
                    new Lesson { Title = "En el Bar", Topic = "Bar", Level = 1 },
                    new Lesson { Title = "Viajes", Topic = "Viajes", Level = 1 }
                };
                
                _context.Lessons.AddRange(lessons);
                await _context.SaveChangesAsync();
                
                foreach (var l in lessons)
                {
                    var exercises = await _aiService.GenerateExercisesAsync(l.Topic, 3);
                    foreach(var ex in exercises) ex.LessonId = l.Id;
                    _context.Exercises.AddRange(exercises);
                }
                await _context.SaveChangesAsync();
            }

            // Seed users and progress for leaderboard if few users exist
            if (await _context.Users.CountAsync() < 5)
            {
                var random = new Random();
                var basqueNames = new[] { "Aitor", "Ane", "Iker", "Maite", "Jon", "Amaia", "Gorka", "Nerea", "Koldo", "Itziar", "Mikel", "Eider", "Unai", "Nagore", "Xabier", "Olatz", "Andoni", "Belen", "Josu", "Arantza" };

                foreach (var name in basqueNames)
                {
                    var user = new User 
                    { 
                        Username = name, 
                        Email = _encryptionService.Encrypt($"{name.ToLower()}@euskalia.eus"),
                        Nickname = _encryptionService.Encrypt(name.ToLower()),
                        Password = _encryptionService.Encrypt("password123"),
                        JoinedAt = DateTime.UtcNow.AddDays(-random.Next(30, 365))
                    };
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    var progress = new Progress
                    {
                        UserId = user.Id,
                        XP = random.Next(100, 5000),
                        WeeklyXP = random.Next(0, 500),
                        MonthlyXP = random.Next(0, 1500),
                        Streak = random.Next(0, 30),
                        Level = random.Next(1, 10),
                        Txanponak = random.Next(50, 1000),
                        LastLessonDate = DateTime.Now.AddDays(-random.Next(1, 7)),
                        LastLessonTitle = "La Comida"
                    };
                    _context.Progresses.Add(progress);
                    await _context.SaveChangesAsync();
                }
            }

            return await _context.Lessons.Include(l => l.Exercises).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Lesson>> GetLesson(int id)
        {
            var lesson = await _context.Lessons.Include(l => l.Exercises).FirstOrDefaultAsync(l => l.Id == id);
            if (lesson == null) return NotFound();
            return lesson;
        }

        [HttpPost("generate-for-topic")]
        public async Task<ActionResult<Lesson>> GenerateLesson(string topic)
        {
            var lesson = new Lesson { Title = topic, Topic = topic, Level = 1 };
            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();

            var exercises = await _aiService.GenerateExercisesAsync(topic, 5);
            foreach(var ex in exercises) ex.LessonId = lesson.Id;
            _context.Exercises.AddRange(exercises);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLesson), new { id = lesson.Id }, lesson);
        }
    }

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

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Cuenta eliminada" });
        }
    }

    public class XPUpdateDto
    {
        public int XP { get; set; }
        public string? LessonTitle { get; set; }
    }

    public class ProfileUpdateDto
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Nickname { get; set; }
        public string? Password { get; set; }
    }
}
