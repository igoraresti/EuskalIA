using EuskalIA.Server.Data;
using EuskalIA.Server.Models;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace EuskalIA.Server.Utils
{
    public static class AigcSeeder
    {
        public static void SeedAigcExercises(AppDbContext context, ILogger logger)
        {
            logger.LogInformation("Starting AIGC exercise seeding.");
            // Resolve the Lessons folder relative to the repo root (2 levels up from
            // the running binary: bin/Debug/net9.0 -> project -> server -> repo root)
            var projectDir = Directory.GetCurrentDirectory();
            var repoRoot = Path.GetFullPath(Path.Combine(projectDir, "..", ".."));
            var lessonsDir = Path.Combine(repoRoot, "Lessons");
 
            SeedFromJsonFile(context, Path.Combine(lessonsDir, "a1_exercises.json"), "A1", logger);
            SeedFromJsonFile(context, Path.Combine(lessonsDir, "a2_exercises.json"), "A2", logger);
            SeedFromJsonFile(context, Path.Combine(lessonsDir, "b1_exercises.json"), "B1", logger);
        }
 
        private static void SeedFromJsonFile(AppDbContext context, string filePath, string level, ILogger logger)
        {
            if (!File.Exists(filePath))
            {
                logger.LogWarning("[AigcSeeder] JSON not found for {Level}: {FilePath}", level, filePath);
                return;
            }

            var jsonContent = File.ReadAllText(filePath);
            var exercises = JsonSerializer.Deserialize<List<AigcExerciseJson>>(jsonContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (exercises == null) return;

            int inserted = 0;
            foreach (var ex in exercises)
            {
                // Skip if already in the database (idempotent by exerciseCode)
                if (context.AigcExercises.Any(e => e.ExerciseCode == ex.ExerciseCode))
                    continue;

                context.AigcExercises.Add(new AigcExercise
                {
                    ExerciseCode = ex.ExerciseCode,
                    TemplateType = ex.TemplateType,
                    LevelId = ex.LevelId ?? level,
                    Topics = ex.Topics ?? "",
                    Difficulty = ex.Difficulty,
                    Status = ex.Status ?? "VERIFIED",
                    JsonSchema = ex.JsonSchema ?? "{}",
                    CreatedAt = DateTime.UtcNow
                });
                inserted++;
            }

            if (inserted > 0)
            {
                context.SaveChanges();
                logger.LogInformation("[AigcSeeder] {Level}: inserted {InsertedCount} exercises.", level, inserted);
            }
            else
            {
                logger.LogInformation("[AigcSeeder] {Level}: all exercises already present, skipped.", level);
            }
        }

        // Minimal DTO for JSON deserialization
        private class AigcExerciseJson
        {
            public string ExerciseCode { get; set; } = "";
            public string TemplateType { get; set; } = "multiple_choice";
            public string? LevelId { get; set; }
            public string? Topics { get; set; }
            public int Difficulty { get; set; } = 1;
            public string? Status { get; set; }
            public string? JsonSchema { get; set; }
        }
    }
}
