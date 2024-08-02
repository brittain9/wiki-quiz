using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Core.Interfaces;

public interface IQuestionResponseRepository
{
    Task<AIResponse> GetByIdAsync(int id);
    Task<IEnumerable<AIResponse>> GetAllAsync();
    Task<AIResponse> AddAsync(AIResponse questionResponse);
    Task UpdateAsync(AIResponse questionResponse);
    Task DeleteAsync(int id);
    Task<IEnumerable<AIResponse>> GetByWikipediaPageIdAsync(int wikipediaPageId);
}
