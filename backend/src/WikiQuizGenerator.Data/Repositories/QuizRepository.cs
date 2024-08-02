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

    public async Task<Quiz> GetByIdAsync(int id)
    {
        return await _context.Quizzes
            .Include(q => q.AIResponses)
                .ThenInclude(qr => qr.Questions)
            .Include(q => q.AIResponses)
                .ThenInclude(qr => qr.WikipediaPage)
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<IEnumerable<Quiz>> GetAllAsync()
    {
        return await _context.Quizzes
            .Include(q => q.AIResponses)
                .ThenInclude(qr => qr.Questions)
            .Include(q => q.AIResponses)
                .ThenInclude(qr => qr.WikipediaPage)
            .ToListAsync();
    }

    public async Task<Quiz> AddAsync(Quiz quiz)
    {
        await _context.Quizzes.AddAsync(quiz);
        await _context.SaveChangesAsync();
        return quiz;
    }

    public async Task UpdateAsync(Quiz quiz)
    {
        _context.Quizzes.Update(quiz);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var quiz = await _context.Quizzes.FindAsync(id);
        if (quiz != null)
        {
            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();
        }
    }
}
