using WikiQuizGenerator.Core.Models;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
            .Include(q => q.AIResponses)
                .ThenInclude(ar => ar.AIMetadata)
            .ToListAsync();
    }

    public async Task<Quiz> GetByIdAsync(int id)
    {
        return await _context.Set<Quiz>()
            .Include(q => q.AIResponses)
                .ThenInclude(ar => ar.Questions)
            .Include(q => q.AIResponses)
                .ThenInclude(ar => ar.AIMetadata)
            .FirstOrDefaultAsync(q => q.Id == id);
    }
}