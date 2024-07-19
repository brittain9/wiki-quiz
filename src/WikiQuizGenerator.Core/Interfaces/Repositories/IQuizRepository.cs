using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Core.Interfaces;

public interface IQuizRepository
{
    Task<Quiz> GetQuizByIdAsync(int id);
    Task<List<Quiz>> GetAllQuizzesAsync();
    Task AddQuizAsync(Quiz quiz);
    Task UpdateQuizAsync(Quiz quiz);
    Task DeleteQuizAsync(int id);
}