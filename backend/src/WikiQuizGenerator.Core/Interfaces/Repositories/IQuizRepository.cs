using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Core.Interfaces;

public interface IQuizRepository
{
    Task<Quiz> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<IEnumerable<Quiz>> GetAllAsync();
    Task<IEnumerable<Submission>> GetRecentQuizSubmissionsAsync(int count = 10);

    Task<Quiz> AddAsync(Quiz quiz);
    Task DeleteAsync(int id);

    Task<Submission?> AddSubmissionAsync(Submission submission, CancellationToken cancellationToken);
    Task DeleteSubmissionAsync(int submissionId);
    Task<Submission> GetSubmissionByIdAsync(int submissionId);
    Task<IEnumerable<Submission>> GetAllSubmissionsAsync();
    Task<IEnumerable<Submission>> GetSubmissionsByUserIdAsync(Guid userId);
    Task<(IEnumerable<Submission> submissions, int totalCount)> GetSubmissionsByUserIdPaginatedAsync(Guid userId, int page, int pageSize);
    Task<Submission?> GetUserSubmissionByIdAsync(int submissionId, Guid userId);
    Task<Submission?> GetUserSubmissionByQuizIdAsync(int quizId, Guid userId, CancellationToken cancellationToken);

}
