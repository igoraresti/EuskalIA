using EuskalIA.Server.Models;

namespace EuskalIA.Server.Services.Interfaces
{
    public interface IAIService
    {
        Task<List<Exercise>> GenerateExercisesAsync(string topic, int count);
    }
}
