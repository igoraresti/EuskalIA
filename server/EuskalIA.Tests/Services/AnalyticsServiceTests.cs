using EuskalIA.Server.Models;
using EuskalIA.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EuskalIA.Tests.Services
{
    public class AnalyticsServiceTests : TestBase
    {
        [Fact]
        public async Task GetUserWeaknessesAsync_ReturnsCorrectAggregates()
        {
            // Arrange
            var context = GetDatabaseContext();
            var userId = 1;
            
            var ex1 = new AigcExercise { Id = Guid.NewGuid(), Topics = "NORK, Verbos", Status = "APPROVED", LevelId = "A1" };
            var ex2 = new AigcExercise { Id = Guid.NewGuid(), Topics = "Saludos, Vocabulario", Status = "APPROVED", LevelId = "A1" };
            var ex3 = new AigcExercise { Id = Guid.NewGuid(), Topics = "NORK", Status = "APPROVED", LevelId = "A1" };
            
            context.AigcExercises.AddRange(ex1, ex2, ex3);
            
            // User failed NORK 2 times (via ex1 and ex3)
            context.UserExerciseAttempts.Add(new UserExerciseAttempt { UserId = userId, ExerciseId = ex1.Id, IsCorrect = false, AttemptDate = DateTime.UtcNow });
            context.UserExerciseAttempts.Add(new UserExerciseAttempt { UserId = userId, ExerciseId = ex3.Id, IsCorrect = false, AttemptDate = DateTime.UtcNow });
            
            // User succeeded in Saludos once
            context.UserExerciseAttempts.Add(new UserExerciseAttempt { UserId = userId, ExerciseId = ex2.Id, IsCorrect = true, AttemptDate = DateTime.UtcNow });
            
            await context.SaveChangesAsync();
            
            var service = new AnalyticsService(context);
            
            // Act
            var result = await service.GetUserWeaknessesAsync(userId);
            
            // Assert
            Assert.NotEmpty(result);
            var nork = result.FirstOrDefault(w => w.Topic == "NORK");
            Assert.NotNull(nork);
            Assert.Equal(2, nork.FailureCount);
            
            // Verbos should also be there because of ex1 (shared with NORK)
            var verbos = result.FirstOrDefault(w => w.Topic == "Verbos");
            Assert.NotNull(verbos);
            Assert.Equal(1, verbos.FailureCount);
            
            // Saludos should NOT be there because it has 0 failures in the result list (we only return failures)
            Assert.Null(result.FirstOrDefault(w => w.Topic == "Saludos"));
        }

        [Fact]
        public async Task GetUserWeaknessesAsync_FiltersByDate()
        {
            // Arrange
            var context = GetDatabaseContext();
            var userId = 1;
            var ex1 = new AigcExercise { Id = Guid.NewGuid(), Topics = "OldTopic", Status = "APPROVED", LevelId = "A1" };
            context.AigcExercises.Add(ex1);
            
            // Attempt older than 30 days
            context.UserExerciseAttempts.Add(new UserExerciseAttempt 
            { 
                UserId = userId, 
                ExerciseId = ex1.Id, 
                IsCorrect = false, 
                AttemptDate = DateTime.UtcNow.AddDays(-31) 
            });
            
            await context.SaveChangesAsync();
            var service = new AnalyticsService(context);
            
            // Act
            var result = await service.GetUserWeaknessesAsync(userId);
            
            // Assert
            Assert.Empty(result);
        }
    }
}
