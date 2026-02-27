using EuskalIA.Server.Data;
using EuskalIA.Server.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.IO;
using System.Collections.Generic;

namespace EuskalIA.Server.Utils
{
    public static class AigcSeeder
    {
        public static void SeedAigcExercises(AppDbContext context)
        {
            // Seed B1 parsing from json if they don't exist
            if (!context.AigcExercises.Any(e => e.LevelId == "B1"))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "b1_exercises.json");
                if (File.Exists(filePath))
                {
                    var jsonContent = File.ReadAllText(filePath);
                    var exercises = JsonSerializer.Deserialize<List<AigcExercise>>(jsonContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (exercises != null)
                    {
                        context.AigcExercises.AddRange(exercises);
                        context.SaveChanges();
                    }
                }
            }

            // Seed A1 test if it doesn't exist
            if (!context.AigcExercises.Any(e => e.ExerciseCode == "test-block-builder-01"))
            {
                var schema = new
                {
                    promptLocal = "Construye la palabra: 'Para el monte'",
                    validation = new
                    {
                        correctSequence = new[] { "root_mendi", "art_a", "des_rentzat" },
                        targetWord = "Mendiarentzat",
                        targetTranslation = "Para el monte",
                        feedback = new
                        {
                            onSuccess = "¡Perfecto!",
                            onFail = "La estructura correcta es: Mendi (íz) + a (artículo) + rentzat (para)."
                        }
                    },
                    elements = new
                    {
                        stem = new
                        {
                            id = "root_mendi",
                            text = "Mendi",
                            type = "noun",
                            colorCode = "#2196F3"
                        },
                        pieces = new[]
                        {
                            new { id = "art_a", text = "a", type = "article", colorCode = "#4CAF50" },
                            new { id = "des_rentzat", text = "rentzat", type = "destination", colorCode = "#FF9800" },
                            new { id = "erg_k", text = "k", type = "ergative", colorCode = "#E91E63" },
                            new { id = "loc_n", text = "n", type = "locative", colorCode = "#9C27B0" }
                        }
                    }
                };

                context.AigcExercises.Add(new AigcExercise
                {
                    ExerciseCode = "test-block-builder-01",
                    TemplateType = "block_builder",
                    LevelId = "A1_UNIT_1",  
                    Topics = "Destination",
                    Difficulty = 1,
                    Status = "VERIFIED",
                    JsonSchema = JsonSerializer.Serialize(schema),
                    CreatedAt = DateTime.UtcNow
                });

                context.SaveChanges();
            }
        }
    }
}
