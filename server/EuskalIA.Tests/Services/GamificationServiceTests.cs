using EuskalIA.Server.Models;
using EuskalIA.Server.Services;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EuskalIA.Tests.Services
{
    public class GamificationServiceTests : TestBase
    {
        private readonly Mock<ILogger<GamificationService>> _mockLogger;

        public GamificationServiceTests()
        {
            _mockLogger = new Mock<ILogger<GamificationService>>();
        }

        [Fact]
        public async Task UpdateStreakAsync_IncrementsStreak_WhenYesterdayWasLastActivity()
        {
            // Arrange
            var context = GetDatabaseContext();
            var userId = 1;
            var progress = new Progress 
            { 
                UserId = userId, 
                Streak = 1, 
                LastLessonDate = DateTime.UtcNow.AddDays(-1) 
            };
            context.Progresses.Add(progress);
            await context.SaveChangesAsync();
            
            var service = new GamificationService(context, _mockLogger.Object);
            
            // Act
            await service.UpdateStreakAsync(userId);
            
            // Assert
            var updatedProgress = context.Progresses.First(p => p.UserId == userId);
            Assert.Equal(2, updatedProgress.Streak);
            Assert.Equal(DateTime.UtcNow.Date, updatedProgress.LastLessonDate.Date);
        }

        [Fact]
        public async Task UpdateStreakAsync_ResetsStreak_WhenInactiveMoreThanOneDay()
        {
            // Arrange
            var context = GetDatabaseContext();
            var userId = 1;
            var progress = new Progress 
            { 
                UserId = userId, 
                Streak = 5, 
                LastLessonDate = DateTime.UtcNow.AddDays(-2) 
            };
            context.Progresses.Add(progress);
            await context.SaveChangesAsync();
            
            var service = new GamificationService(context, _mockLogger.Object);
            
            // Act
            await service.UpdateStreakAsync(userId);
            
            // Assert
            var updatedProgress = context.Progresses.First(p => p.UserId == userId);
            Assert.Equal(1, updatedProgress.Streak);
        }

        [Fact]
        public async Task CheckAchievementsAsync_UnlocksAchievement_WhenConditionsMet()
        {
            // Arrange
            var context = GetDatabaseContext();
            var userId = 1;
            var user = new User { Id = userId, Username = "Tester" };
            var progress = new Progress { UserId = userId, XP = 1000, Level = 5 };
            
            var ach = new Achievement 
            { 
                Code = "MAESTRO_5",
                Name = "Maestro", 
                Category = "XP", 
                TargetValue = 1000,
                Description = "Llega a 1000 XP"
            };
            
            context.Users.Add(user);
            context.Progresses.Add(progress);
            context.Achievements.Add(ach);
            await context.SaveChangesAsync();
            
            var service = new GamificationService(context, _mockLogger.Object);
            
            // Act
            var newlyEarned = await service.CheckAchievementsAsync(userId);
            
            // Assert
            Assert.NotEmpty(newlyEarned);
            Assert.Contains(newlyEarned, a => a.Name == "Maestro");
            
            var userAch = context.UserAchievements.FirstOrDefault(ua => ua.UserId == userId && ua.AchievementId == ach.Id);
            Assert.NotNull(userAch);
        }
    }
}
