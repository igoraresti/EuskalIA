using Microsoft.EntityFrameworkCore;
using EuskalIA.Server.Models;
using Microsoft.Extensions.Logging;

namespace EuskalIA.Server.Data
{
    public class AppDbContext : DbContext
    {
        private readonly ILogger<AppDbContext> _logger;

        public AppDbContext(DbContextOptions<AppDbContext> options, ILogger<AppDbContext> logger) : base(options)
        {
            _logger = logger;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Progress> Progresses { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<LessonProgress> LessonProgresses { get; set; }
        public DbSet<UserSrsNode> UserSrsNodes { get; set; }
        public DbSet<AigcExercise> AigcExercises { get; set; }
        public DbSet<UserExerciseAttempt> UserExerciseAttempts { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<UserAchievement> UserAchievements { get; set; }
        public DbSet<BookProgress> BookProgresses { get; set; }
        public DbSet<AigcLog> AigcLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _logger.LogInformation("Configuring database model in OnModelCreating.");
            modelBuilder.Entity<User>()
                .HasOne(u => u.Progress)
                .WithOne(p => p.User)
                .HasForeignKey<Progress>(p => p.UserId);

            modelBuilder.Entity<Lesson>()
                .HasMany(l => l.Exercises)
                .WithOne(e => e.Lesson)
                .HasForeignKey(e => e.LessonId);

            // Seed Achievements
            modelBuilder.Entity<Achievement>().HasData(
                new Achievement { Id = 1, Code = "MADRUGADOR", Name = "Madrugador", Description = "Completa tu primera lección", Icon = "Sun", Category = "LESSONS", TargetValue = 1 },
                new Achievement { Id = 2, Code = "CONSTANCIA", Name = "Constancia", Description = "Alcanza una racha de 7 días", Icon = "Flame", Category = "STREAK", TargetValue = 7 },
                new Achievement { Id = 3, Code = "ERUDITO", Name = "Erudito", Description = "Consigue 1000 XP totales", Icon = "BookOpen", Category = "XP", TargetValue = 1000 },
                new Achievement { Id = 4, Code = "MAESTRO", Name = "Maestro", Description = "Completa 10 lecciones", Icon = "Award", Category = "LESSONS", TargetValue = 10 }
            );
        }

        public override int SaveChanges()
        {
            _logger.LogInformation("Saving changes to the database.");
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Saving changes to the database asynchronously.");
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
