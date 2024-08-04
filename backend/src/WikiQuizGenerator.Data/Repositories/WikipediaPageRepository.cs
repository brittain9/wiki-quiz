using WikiQuizGenerator.Core.Models;
using WikiQuizGenerator.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using WikiQuizGenerator.Core;

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
        return await _context.Set<WikipediaPage>()
            .Include(wp => wp.Categories)
            .FirstOrDefaultAsync(wp => wp.Id == id);
    }

    public async Task<WikipediaPage> GetByWikipediaIdAsync(int wikipediaId, Languages language)
    {
        // different languages can link to the same wikipedia id
        var lang = LanguagesExtensions.GetWikipediaLanguageCode(language);
        return await _context.Set<WikipediaPage>()
            .Where(wp => wp.Language == lang)
            .Include(wp => wp.Categories)
            .FirstOrDefaultAsync(wp => wp.WikipediaId == wikipediaId);
    }

    public async Task<IEnumerable<WikipediaPage>> GetAllAsync()
    {
        return await _context.Set<WikipediaPage>()
            .Include(wp => wp.Categories)
            .ToListAsync();
    }

    public async Task<WikipediaPage> AddAsync(WikipediaPage wikipediaPage)
    {
        await _context.Set<WikipediaPage>().AddAsync(wikipediaPage);
        await _context.SaveChangesAsync();
        return wikipediaPage;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var wikipediaPage = await _context.Set<WikipediaPage>()
            .Include(w => w.AIResponses)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (wikipediaPage == null)
        {
            return false; // Page not found
        }

        // Check if there are any associated AIResponses
        if (wikipediaPage.AIResponses.Any())
        {
            return false; // Don't delete if there are associated AIResponses
        }

        // If no AIResponses, proceed with deletion
        _context.WikipediaPages.Remove(wikipediaPage);
        await _context.SaveChangesAsync();
        return true; // Deletion successful
    }


    public async Task<WikipediaPage> GetByTitleAsync(string title, Languages language)
    {
        string languageCode = language.GetWikipediaLanguageCode();
        return await _context.WikipediaPages
            .Include(wp => wp.Categories)
            .FirstOrDefaultAsync(wp => wp.Title == title && wp.Language == languageCode);
    }

    public async Task<bool> ExistsByTitleAsync(string title, Languages language)
    {
        string languageCode = language.GetWikipediaLanguageCode();
        return await _context.Set<WikipediaPage>()
            .AnyAsync(wp => wp.Title == title && wp.Language == languageCode);
    }

    public async Task<IEnumerable<WikipediaPage>> GetByLanguageAsync(string language)
    {
        return await _context.Set<WikipediaPage>()
            .Where(wp => wp.Language == language)
            .Include(wp => wp.Categories)
            .ToListAsync();
    }

    public async Task<IEnumerable<WikipediaPage>> GetByCategoryAsync(string categoryName)
    {
        return await _context.Set<WikipediaPage>()
            .Where(wp => wp.Categories.Any(c => c.Name == categoryName))
            .Include(wp => wp.Categories)
            .ToListAsync();
    }

    public async Task DeleteEmptyCategoriesAsync()
    {
        var emptyCategories = await _context.WikipediaCategories
            .Where(c => !c.WikipediaPages.Any())
            .ToListAsync();

        if (emptyCategories.Any())
        {
            _context.WikipediaCategories.RemoveRange(emptyCategories);
            await _context.SaveChangesAsync();
        }
    }
}