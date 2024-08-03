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
        return await _context.Set<WikipediaPage>()
            .Include(wp => wp.Links)
            .Include(wp => wp.Categories)
            .FirstOrDefaultAsync(wp => wp.Id == id);
    }

    public async Task<IEnumerable<WikipediaPage>> GetAllAsync()
    {
        return await _context.Set<WikipediaPage>()
            .Include(wp => wp.Links)
            .Include(wp => wp.Categories)
            .ToListAsync();
    }

    public async Task<WikipediaPage> AddAsync(WikipediaPage wikipediaPage)
    {
        await _context.Set<WikipediaPage>().AddAsync(wikipediaPage);
        await _context.SaveChangesAsync();
        return wikipediaPage;
    }

    public async Task UpdateAsync(WikipediaPage wikipediaPage)
    {
        _context.Set<WikipediaPage>().Update(wikipediaPage);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var wikipediaPage = await _context.Set<WikipediaPage>().FindAsync(id);
        if (wikipediaPage != null)
        {
            _context.Set<WikipediaPage>().Remove(wikipediaPage);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<WikipediaPage> GetByTitleAsync(string title)
    {
        var wikipediaPage = await _context.Set<WikipediaPage>()
            .Include(wp => wp.Links)
            .Include(wp => wp.Categories)
            .FirstOrDefaultAsync(wp => wp.Title == title);

        if (wikipediaPage == null)
        {
            throw new KeyNotFoundException($"WikipediaPage with title '{title}' not found.");
        }

        return wikipediaPage;
    }

    public async Task<bool> ExistsByTitleAsync(string title)
    {
        return await _context.Set<WikipediaPage>()
            .AnyAsync(wp => wp.Title == title);
    }

    public async Task<IEnumerable<WikipediaPage>> GetByLanguageAsync(string language)
    {
        return await _context.Set<WikipediaPage>()
            .Where(wp => wp.Language == language)
            .Include(wp => wp.Links)
            .Include(wp => wp.Categories)
            .ToListAsync();
    }

    public async Task<IEnumerable<WikipediaPage>> GetByCategoryAsync(string categoryName)
    {
        return await _context.Set<WikipediaPage>()
            .Where(wp => wp.Categories.Any(c => c.Name == categoryName))
            .Include(wp => wp.Links)
            .Include(wp => wp.Categories)
            .ToListAsync();
    }

    public async Task AddLinkAsync(int pageId, WikipediaLink link)
    {
        var page = await _context.Set<WikipediaPage>().FindAsync(pageId);
        if (page != null)
        {
            link.WikipediaPageId = pageId;
            await _context.Set<WikipediaLink>().AddAsync(link);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveLinkAsync(int pageId, int linkId)
    {
        var link = await _context.Set<WikipediaLink>()
            .FirstOrDefaultAsync(l => l.Id == linkId && l.WikipediaPageId == pageId);

        if (link != null)
        {
            _context.Set<WikipediaLink>().Remove(link);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddCategoryAsync(int pageId, string categoryName)
    {
        var page = await _context.Set<WikipediaPage>().FindAsync(pageId);
        var category = await _context.Set<WikipediaCategory>()
            .FirstOrDefaultAsync(c => c.Name == categoryName);

        if (page != null)
        {
            if (category == null)
            {
                category = new WikipediaCategory { Name = categoryName };
                await _context.Set<WikipediaCategory>().AddAsync(category);
                await _context.SaveChangesAsync();
            }

            if (!page.Categories.Any(c => c.Id == category.Id))
            {
                page.Categories.Add(category);
                await _context.SaveChangesAsync();
            }
        }
    }

    public async Task RemoveCategoryAsync(int pageId, string categoryName)
    {
        var page = await _context.Set<WikipediaPage>()
            .Include(wp => wp.Categories)
            .FirstOrDefaultAsync(wp => wp.Id == pageId);

        if (page != null)
        {
            var category = page.Categories.FirstOrDefault(c => c.Name == categoryName);
            if (category != null)
            {
                page.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }
    }
}