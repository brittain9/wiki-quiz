using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Data;

public class QuizRepository : IQuizRepository
{
    private readonly WikiQuizDbContext _context;

    public QuizRepository(WikiQuizDbContext context)
    {
        _context = context;
    }
    public Task<Quiz> GetQuizByIdAsync(int id)
    {
        throw new NotImplementedException();
    }
    public Task<List<Quiz>> GetAllQuizzesAsync()
    {
        throw new NotImplementedException();
    }
    public Task AddQuizAsync(Quiz quiz)
    {
        throw new NotImplementedException();
    }
    public Task UpdateQuizAsync(Quiz quiz)
    {
        throw new NotImplementedException();    
    }
    public Task DeleteQuizAsync(int id)
    {
        throw new NotImplementedException();
    }
}