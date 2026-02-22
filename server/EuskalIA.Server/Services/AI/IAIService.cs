using EuskalIA.Server.Models;

namespace EuskalIA.Server.Services.AI
{
    public interface IAIService
    {
        Task<List<Exercise>> GenerateExercisesAsync(string topic, int count);
    }
}
