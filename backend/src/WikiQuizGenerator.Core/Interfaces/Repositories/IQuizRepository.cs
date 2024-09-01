using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Core.Interfaces;

public interface IQuizRepository
{
    Task<Quiz> GetByIdAsync(int id);
    Task<IEnumerable<Quiz>> GetAllAsync();
    Task<IEnumerable<Submission>> GetRecentQuizSubmissionsAsync(int count = 10);

    Task<Quiz> AddAsync(Quiz quiz);
    Task DeleteAsync(int id);

    Task<Submission?> AddSubmissionAsync(Submission submission);
    Task DeleteSubmissionAsync(int submissionId);
    Task<Submission> GetSubmissionByIdAsync(int submissionId);
    Task<IEnumerable<Submission>> GetAllSubmissionsAsync();
}
