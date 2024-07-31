using WikiQuizGenerator.Core.Models;
using WikiQuizGenerator.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace WikiQuizGenerator.Data.Repositories;

public class WikipediaPageRepository : IWikipediaPageRepository
{
    private readonly WikiQuizDbContext _context;

    public WikipediaPageRepository(WikiQuizDbContext context)
    {
        _context = context;
    }

    public async Task<WikipediaPage> GetByIdAsync(int id)
    {
        return await _context.WikipediaPages.FindAsync(id);
    }

    public async Task<IEnumerable<WikipediaPage>> GetAllAsync()
    {
        return await _context.WikipediaPages.ToListAsync();
    }

    public async Task<WikipediaPage> AddAsync(WikipediaPage wikipediaPage)
    {
        await _context.WikipediaPages.AddAsync(wikipediaPage);
        await _context.SaveChangesAsync();
        return wikipediaPage;
    }

    public async Task UpdateAsync(WikipediaPage wikipediaPage)
    {
        _context.Entry(wikipediaPage).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var wikipediaPage = await _context.WikipediaPages.FindAsync(id);
        if (wikipediaPage != null)
        {
            _context.WikipediaPages.Remove(wikipediaPage);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<WikipediaPage> GetByTitleAsync(string title)
    {
        return await _context.WikipediaPages
            .FirstOrDefaultAsync(wp => wp.Title == title);
    }
}
