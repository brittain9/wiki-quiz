using WikiQuizGenerator.Core.Models;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Data;
using Microsoft.EntityFrameworkCore;

namespace WikiQuizGenerator.Data.Repositories;

public class QuizRepository : IQuizRepository
{
    private readonly WikiQuizDbContext _context;

    public QuizRepository(WikiQuizDbContext context)
    {
        _context = context;
    }

    public Task<Quiz> AddAsync(Quiz quiz)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Quiz>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Quiz> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }
}
