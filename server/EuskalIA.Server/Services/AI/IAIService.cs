using EuskalIA.Server.Models;

namespace EuskalIA.Server.Services.AI
{
    public interface IAIService
    {
        Task<List<Exercise>> GenerateExercisesAsync(string topic, int count);
        Task<List<AigcExercise>> GenerateAigcExercisesAsync(string levelId, string context, int count);
    }
}
