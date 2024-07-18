using WikiDotNet;
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
    
    public string GetArticleContent(string topic)
    {
        WikiSearcher searcher = new();
         WikiSearchSettings searchSettings = new()
            { RequestId = "Request ID", ResultLimit = 1, ResultOffset = 2, Language = "en" };
        WikiSearchResponse response = searcher.Search(topic, searchSettings);

        return response.Query.SearchResults[0].Preview;
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