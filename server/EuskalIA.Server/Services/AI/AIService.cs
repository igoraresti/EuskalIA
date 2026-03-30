using System.Text.Json;
using EuskalIA.Server.Models;
namespace EuskalIA.Server.Services.AI
{
    public class MockAIService : IAIService
    {
        public Task<List<Exercise>> GenerateExercisesAsync(string topic, int count)
        {
            // Simulate AI delay
            var exercises = new List<Exercise>();
            
            if (topic.ToLower() == "saludos")
            {
                exercises.Add(new Exercise 
                { 
                    Type = "MultipleChoice", 
                    Question = "¿Cómo se dice 'Hola'?", 
                    CorrectAnswer = "Kaixo", 
                    OptionsJson = "[\"Kaixo\", \"Agur\", \"Egun on\", \"Arratsalde on\"]"
                });
                exercises.Add(new Exercise 
                { 
                    Type = "MultipleChoice", 
                    Question = "¿Cómo se dice 'Adiós'?", 
                    CorrectAnswer = "Agur", 
                    OptionsJson = "[\"Kaixo\", \"Agur\", \"Ezkerrerik asko\", \"Mesedez\"]"
                });
            }
            else
            {
                // Generic mock for other topics
                for (int i = 0; i < count; i++)
                {
                    exercises.Add(new Exercise 
                    { 
                        Type = "MultipleChoice", 
                        Question = $"Pregunta IA sobre {topic} #{i+1}", 
                        CorrectAnswer = "Opción A", 
                        OptionsJson = "[\"Opción A\", \"Opción B\", \"Opción C\", \"Opción D\"]"
                    });
                }
            }

            return Task.FromResult(exercises);
        }

        public Task<List<AigcExercise>> GenerateAigcExercisesAsync(string levelId, string context, int count)
        {
            var list = new List<AigcExercise>();
            for (int i = 0; i < count; i++)
            {
                list.Add(new AigcExercise { 
                    ExerciseCode = $"mock_{levelId}_{Guid.NewGuid().ToString().Substring(0,4)}",
                    LevelId = levelId,
                    TemplateType = "multiple_choice",
                    JsonSchema = "{\"question\": {\"es\": \"Pregunta Mock\"}, \"options\": [\"A\", \"B\"], \"correctAnswer\": \"A\"}",
                    Status = "BETA"
                });
            }
            return Task.FromResult(list);
        }
    }
}
