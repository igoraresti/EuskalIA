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

        public LessonsController(AppDbContext context, IAIService aiService)
        {
            _context = context;
            _aiService = aiService;
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

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}/progress")]
        public async Task<ActionResult<Progress>> GetProgress(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                // Create user if not exists for ID 1 (MVP shortcut)
                user = new User { Id = id, Username = "Usuario", Email = "test@euskalia.com" };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            var progress = await _context.Progresses.FirstOrDefaultAsync(p => p.UserId == id);
            if (progress == null)
            {
                // Create initial progress if not exists
                progress = new Progress { UserId = id, XP = 0, Level = 1, Streak = 0, Txanponak = 100 };
                _context.Progresses.Add(progress);
                await _context.SaveChangesAsync();
            }
            return progress;
        }

        [HttpPost("{id}/xp")]
        public async Task<IActionResult> AddXP(int id, [FromBody] int xp)
        {
            var progress = await _context.Progresses.FirstOrDefaultAsync(p => p.UserId == id);
            if (progress == null) return NotFound();

            progress.XP += xp;
            // Simple level logic
            progress.Level = (progress.XP / 1000) + 1;
            
            await _context.SaveChangesAsync();
            return Ok(progress);
        }
    }
}
