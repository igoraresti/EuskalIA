using Microsoft.EntityFrameworkCore;
using EuskalIA.Server.Models;

namespace EuskalIA.Server.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Progress> Progresses { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<LessonProgress> LessonProgresses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasOne(u => u.Progress)
                .WithOne(p => p.User)
                .HasForeignKey<Progress>(p => p.UserId);

            modelBuilder.Entity<Lesson>()
                .HasMany(l => l.Exercises)
                .WithOne(e => e.Lesson)
                .HasForeignKey(e => e.LessonId);
        }
    }
}
