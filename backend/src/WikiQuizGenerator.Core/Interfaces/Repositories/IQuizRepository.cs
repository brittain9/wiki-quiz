using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Core.Interfaces;

public interface IQuizRepository
{
    Task<Quiz> GetByIdAsync(int id);
    Task<IEnumerable<Quiz>> GetAllAsync();
    Task<Quiz> AddAsync(Quiz quiz);
    Task UpdateAsync(Quiz quiz);
    Task DeleteAsync(int id);
}
