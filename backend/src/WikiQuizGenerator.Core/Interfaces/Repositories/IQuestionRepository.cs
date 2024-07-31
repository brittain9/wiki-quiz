using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Core.Interfaces;

public interface IQuestionRepository
{
    Task<Question> GetByIdAsync(int id);
    Task<IEnumerable<Question>> GetAllAsync();
    Task<Question> AddAsync(Question question);
    Task UpdateAsync(Question question);
    Task DeleteAsync(int id);
    Task<IEnumerable<Question>> GetByQuestionResponseIdAsync(int questionResponseId);
}