using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Core.Interfaces;

public interface IQuestionResponseRepository
{
    Task<QuestionResponse> GetByIdAsync(int id);
    Task<IEnumerable<QuestionResponse>> GetAllAsync();
    Task<QuestionResponse> AddAsync(QuestionResponse questionResponse);
    Task UpdateAsync(QuestionResponse questionResponse);
    Task DeleteAsync(int id);
    Task<IEnumerable<QuestionResponse>> GetByWikipediaPageIdAsync(int wikipediaPageId);
}
