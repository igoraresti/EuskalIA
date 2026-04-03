using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EuskalIA.Server.Data;
using EuskalIA.Server.Models;
using EuskalIA.Server.Services.AI;
using EuskalIA.Server.Services.Encryption;

using Microsoft.AspNetCore.Authorization;

namespace EuskalIA.Server.Controllers
{
    /// <summary>
    /// Controller for managing structured lessons and generating new lesson content.
    /// Includes functionality for seeding initial lesson and user data.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/euskalia/[controller]")]
    public class LessonsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAIService _aiService;
        private readonly IEncryptionService _encryptionService;
        private readonly ILogger<LessonsController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="LessonsController"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="aiService">The AI service for generating lesson exercises.</param>
        /// <param name="encryptionService">The encryption service for handling user data during seeding.</param>
        /// <param name="logger">The controller logger.</param>
        public LessonsController(AppDbContext context, IAIService aiService, IEncryptionService encryptionService, ILogger<LessonsController> logger)
        {
            _context = context;
            _aiService = aiService;
            _encryptionService = encryptionService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all available lessons.
        /// Triggers data seeding for lessons and sample users if the database is empty or sparse.
        /// </summary>
        /// <returns>A collection of <see cref="Lesson"/> objects including their associated exercises.</returns>
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

        /// <summary>
        /// Retrieves a specific lesson by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the lesson.</param>
        /// <returns>A <see cref="Lesson"/> object if found; otherwise, NotFound.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Lesson>> GetLesson(int id)
        {
            var lesson = await _context.Lessons.Include(l => l.Exercises).FirstOrDefaultAsync(l => l.Id == id);
            if (lesson == null) return NotFound();
            return lesson;
        }

        /// <summary>
        /// Generates a new lesson and its associated exercises for a specific topic using AI.
        /// </summary>
        /// <param name="topic">The topic to generate the lesson for.</param>
        /// <returns>The newly created <see cref="Lesson"/> object.</returns>
        [HttpPost("generate-for-topic")]
        public async Task<ActionResult<Lesson>> GenerateLesson(string topic)
        {
            _logger.LogInformation("Generating new lesson for topic {Topic}.", topic);
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
}
