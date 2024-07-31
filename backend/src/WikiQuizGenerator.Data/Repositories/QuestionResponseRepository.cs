using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Models;
using WikiQuizGenerator.Data;
using Microsoft.EntityFrameworkCore;

namespace WikiQuizGenerator.Data.Repositories;

public class QuestionResponseRepository : IQuestionResponseRepository
{
    private readonly WikiQuizDbContext _context;

    public QuestionResponseRepository(WikiQuizDbContext context)
    {
        _context = context;
    }

    public async Task<QuestionResponse> GetByIdAsync(int id)
    {
        return await _context.QuestionResponses
            .Include(qr => qr.Questions)
            .Include(qr => qr.WikipediaPage)
            .FirstOrDefaultAsync(qr => qr.Id == id);
    }

    public async Task<IEnumerable<QuestionResponse>> GetAllAsync()
    {
        return await _context.QuestionResponses
            .Include(qr => qr.Questions)
            .Include(qr => qr.WikipediaPage)
            .ToListAsync();
    }

    public async Task<QuestionResponse> AddAsync(QuestionResponse questionResponse)
    {
        await _context.QuestionResponses.AddAsync(questionResponse);
        await _context.SaveChangesAsync();
        return questionResponse;
    }

    public async Task UpdateAsync(QuestionResponse questionResponse)
    {
        _context.QuestionResponses.Update(questionResponse);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var questionResponse = await _context.QuestionResponses.FindAsync(id);
        if (questionResponse != null)
        {
            _context.QuestionResponses.Remove(questionResponse);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<QuestionResponse>> GetByWikipediaPageIdAsync(int wikipediaPageId)
    {
        return await _context.QuestionResponses
            .Include(qr => qr.Questions)
            .Include(qr => qr.WikipediaPage)
            .Where(qr => qr.WikipediaPageId == wikipediaPageId)
            .ToListAsync();
    }
}
