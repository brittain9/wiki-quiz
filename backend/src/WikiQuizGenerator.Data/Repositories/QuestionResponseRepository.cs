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

    public async Task<AIResponse> GetByIdAsync(int id)
    {
        return await _context.AIResponses
            .Include(qr => qr.Questions)
            .Include(qr => qr.WikipediaPage)
            .FirstOrDefaultAsync(qr => qr.Id == id);
    }

    public async Task<IEnumerable<AIResponse>> GetAllAsync()
    {
        return await _context.AIResponses
            .Include(qr => qr.Questions)
            .Include(qr => qr.WikipediaPage)
            .ToListAsync();
    }

    public async Task<AIResponse> AddAsync(AIResponse questionResponse)
    {
        await _context.AIResponses.AddAsync(questionResponse);
        await _context.SaveChangesAsync();
        return questionResponse;
    }

    public async Task UpdateAsync(AIResponse questionResponse)
    {
        _context.AIResponses.Update(questionResponse);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var questionResponse = await _context.AIResponses.FindAsync(id);
        if (questionResponse != null)
        {
            _context.AIResponses.Remove(questionResponse);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<AIResponse>> GetByWikipediaPageIdAsync(int wikipediaPageId)
    {
        return await _context.AIResponses
            .Include(qr => qr.Questions)
            .Include(qr => qr.WikipediaPage)
            .Where(qr => qr.WikipediaPageId == wikipediaPageId)
            .ToListAsync();
    }
}
