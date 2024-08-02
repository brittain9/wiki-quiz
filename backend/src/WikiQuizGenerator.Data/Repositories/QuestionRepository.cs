using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Models;
using WikiQuizGenerator.Data;
using Microsoft.EntityFrameworkCore;

namespace WikiQuizGenerator.Data.Repositories;

public class QuestionRepository : IQuestionRepository
{
    private readonly WikiQuizDbContext _context;

    public QuestionRepository(WikiQuizDbContext context)
    {
        _context = context;
    }

    public async Task<Question> GetByIdAsync(int id)
    {
        return await _context.Questions
            .Include(q => q.AIResponse)
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<IEnumerable<Question>> GetAllAsync()
    {
        return await _context.Questions
            .Include(q => q.AIResponse)
            .ToListAsync();
    }

    public async Task<Question> AddAsync(Question question)
    {
        await _context.Questions.AddAsync(question);
        await _context.SaveChangesAsync();
        return question;
    }

    public async Task UpdateAsync(Question question)
    {
        _context.Questions.Update(question);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var question = await _context.Questions.FindAsync(id);
        if (question != null)
        {
            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Question>> GetByQuestionResponseIdAsync(int questionResponseId)
    {
        return await _context.Questions
            .Where(q => q.AIResponseId == questionResponseId)
            .ToListAsync();
    }
}
