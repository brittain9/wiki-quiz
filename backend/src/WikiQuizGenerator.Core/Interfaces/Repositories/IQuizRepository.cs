using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Core.Interfaces;

public interface IQuizRepository
{
    Task<Quiz> GetByIdAsync(int id);
    Task<IEnumerable<Quiz>> GetAllAsync();
    Task<IEnumerable<QuizSubmission>> GetRecentQuizSubmissionsAsync(int count = 10);

    Task<Quiz> AddAsync(Quiz quiz);
    Task DeleteAsync(int id);

    Task<QuizSubmission?> AddSubmissionAsync(QuizSubmission submission);
    Task DeleteSubmissionAsync(int submissionId);
    Task<QuizSubmission> GetSubmissionByIdAsync(int submissionId);
    Task<IEnumerable<QuizSubmission>> GetAllSubmissionsAsync();
}
