using EuskalIA.Server.Data;
using EuskalIA.Server.Models;
using EuskalIA.Server.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace EuskalIA.Tests.Utils
{
    public class AigcSeederTests : TestBase
    {
        [Fact]
        public void SeedFromJsonFile_InsertsNewExercises()
        {
            // Arrange
            var context = GetDatabaseContext();
            var logger = new Mock<ILogger>().Object;
            var level = "A1";
            var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");

            var exercises = new List<object>
            {
                new { ExerciseCode = "ex1", TemplateType = "T1", LevelId = "A1", Topics = "T", Difficulty = 1, Status = "APPROVED", JsonSchema = "{}" },
                new { ExerciseCode = "ex2", TemplateType = "T2", LevelId = "A1", Topics = "T", Difficulty = 2, Status = "APPROVED", JsonSchema = "{}" }
            };
            File.WriteAllText(filePath, JsonSerializer.Serialize(exercises));

            try
            {
                // Act
                // Since SeedFromJsonFile is private, we use reflection or test the public method.
                // But SeedAigcExercises is also static and hardcodes paths.
                // Let's use reflection for the private method to get 100% on it.
                var method = typeof(AigcSeeder).GetMethod("SeedFromJsonFile", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                method!.Invoke(null, new object[] { context, filePath, level, logger });

                // Assert
                Assert.Equal(2, context.AigcExercises.Count());
                Assert.Contains(context.AigcExercises.ToList(), e => e.ExerciseCode == "ex1");
                Assert.Contains(context.AigcExercises.ToList(), e => e.ExerciseCode == "ex2");
            }
            finally
            {
                if (File.Exists(filePath)) File.Delete(filePath);
            }
        }

        [Fact]
        public void SeedFromJsonFile_IsIdempotent()
        {
            // Arrange
            var context = GetDatabaseContext();
            var logger = new Mock<ILogger>().Object;
            var level = "A1";
            var filePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");

            context.AigcExercises.Add(new AigcExercise { ExerciseCode = "ex1", LevelId = "A1", TemplateType = "T1", JsonSchema = "{}" });
            context.SaveChanges();

            var exercises = new List<object>
            {
                new { ExerciseCode = "ex1", TemplateType = "T1", LevelId = "A1", Topics = "T" }
            };
            File.WriteAllText(filePath, JsonSerializer.Serialize(exercises));

            try
            {
                // Act
                var method = typeof(AigcSeeder).GetMethod("SeedFromJsonFile", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                method!.Invoke(null, new object[] { context, filePath, level, logger });

                // Assert: Still only 1 exercise
                Assert.Equal(1, context.AigcExercises.Count());
            }
            finally
            {
                if (File.Exists(filePath)) File.Delete(filePath);
            }
        }
    }
}
