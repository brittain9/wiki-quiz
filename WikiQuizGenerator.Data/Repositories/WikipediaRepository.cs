using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Data;
public class WikipediaRepository : IWikipediaRepository
{
    private readonly ApplicationDbContext _context;

    public WikipediaRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<string> GetRandomArticleContentAsync()
    {
        // Placeholder implementation
        return @"Sample Wikipedia article content.";
    }

    public Task<CachedWikipediaData> GetCachedArticleAsync(string title)
    {
        throw new NotImplementedException();
    }
    public Task CacheArticleAsync(CachedWikipediaData data)
    {
        throw new NotImplementedException();
    }
}