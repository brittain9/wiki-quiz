using Microsoft.EntityFrameworkCore;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Data.Repositories;

public class QuizRepository : IQuizRepository
{
    private readonly WikiQuizDbContext _context;

    public QuizRepository(WikiQuizDbContext context)
    {
        _context = context;
    }

    public async Task<Quiz> AddAsync(Quiz quiz)
    {
        await _context.Set<Quiz>().AddAsync(quiz);
        await _context.SaveChangesAsync();
        return quiz;
    }

    public async Task DeleteAsync(int id)
    {
        var quiz = await _context.Set<Quiz>().FindAsync(id);
        if (quiz != null)
        {
            _context.Set<Quiz>().Remove(quiz);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Quiz>> GetAllAsync()
    {
        return await _context.Set<Quiz>()
            .Include(q => q.AIResponses)
                .ThenInclude(ar => ar.Questions)
            .ToListAsync();
    }

    public async Task<Quiz> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _context.Set<Quiz>()
            .Include(q => q.AIResponses)
                .ThenInclude(ar => ar.Questions)
            .Include(q => q.AIResponses)
                .ThenInclude(ar => ar.WikipediaPage)
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<Submission> AddSubmissionAsync(Submission submission, CancellationToken cancellationToken)
    {
        _context.QuizSubmissions.Add(submission);
        await _context.SaveChangesAsync();
        return submission;
    }

    public async Task DeleteSubmissionAsync(int submissionId)
    {
        var submission = await _context.QuizSubmissions.FindAsync(submissionId);
        if (submission != null)
        {
            _context.QuizSubmissions.Remove(submission);
            await _context.SaveChangesAsync();

        }
    }

    public async Task<IEnumerable<Submission>> GetRecentQuizSubmissionsAsync(int count)
    {
        var recentSubmissions = await _context.Set<Submission>()
            .AsNoTracking()
            .OrderByDescending(q => q.SubmissionTime)
            .Take(count)
            .Include(qs => qs.Quiz)
            .ToListAsync();

        return recentSubmissions ?? new List<Submission>();
    }

    public async Task<Submission?> GetSubmissionByIdAsync(int submissionId)
    {
        return await _context.QuizSubmissions
            .Include(qs => qs.Quiz).ThenInclude(q => q.AIResponses).ThenInclude(a => a.Questions)
            .Include(qs => qs.Quiz).ThenInclude(q => q.AIResponses).ThenInclude(a => a.WikipediaPage)
            .Include(qs => qs.Answers)
            .Include(qs => qs.User)
            .FirstOrDefaultAsync(qs => qs.Id == submissionId);
    }

    public async Task<IEnumerable<Submission>> GetAllSubmissionsAsync()
    {
        return await _context.QuizSubmissions
            .Include(qs => qs.Quiz).ThenInclude(q => q.AIResponses).ThenInclude(a => a.Questions)
            .Include(qs => qs.Answers)
            .Include(qs => qs.User)
            .ToListAsync();
    }

    public async Task<IEnumerable<Submission>> GetSubmissionsByUserIdAsync(Guid userId)
    {
        return await _context.QuizSubmissions
            .Where(s => s.UserId == userId)
            .Include(qs => qs.Quiz).ThenInclude(q => q.AIResponses).ThenInclude(a => a.Questions)
            .Include(qs => qs.Answers)
            .Include(qs => qs.User)
            .OrderByDescending(s => s.SubmissionTime)
            .ToListAsync();
    }

    public async Task<(IEnumerable<Submission> submissions, int totalCount)> GetSubmissionsByUserIdPaginatedAsync(Guid userId, int page, int pageSize)
    {
        var query = _context.QuizSubmissions
            .Where(s => s.UserId == userId)
            .Include(qs => qs.Quiz).ThenInclude(q => q.AIResponses).ThenInclude(a => a.Questions)
            .Include(qs => qs.Answers)
            .Include(qs => qs.User)
            .OrderByDescending(s => s.SubmissionTime);

        var totalCount = await query.CountAsync();
        
        var submissions = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (submissions, totalCount);
    }

    public async Task<Submission?> GetUserSubmissionByIdAsync(int submissionId, Guid userId)
    {
        return await _context.QuizSubmissions
            .Include(qs => qs.Quiz).ThenInclude(q => q.AIResponses).ThenInclude(a => a.Questions)
            .Include(qs => qs.Quiz).ThenInclude(q => q.AIResponses).ThenInclude(a => a.WikipediaPage)
            .Include(qs => qs.Answers)
            .Include(qs => qs.User)
            .FirstOrDefaultAsync(qs => qs.Id == submissionId && qs.UserId == userId);
    }

    public async Task<Submission?> GetUserSubmissionByQuizIdAsync(int quizId, Guid userId, CancellationToken cancellationToken)
    {
        return await _context.QuizSubmissions
            .FirstOrDefaultAsync(s => s.QuizId == quizId && s.UserId == userId);
    }

    public async Task<ModelConfig?> GetModelConfigByIdAsync(int modelConfigId)
    {
        return await _context.ModelConfigs
            .FirstOrDefaultAsync(m => m.Id == modelConfigId);
    }
}
