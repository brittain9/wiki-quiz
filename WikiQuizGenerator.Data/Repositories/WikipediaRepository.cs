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
            { RequestId = "Request ID", ResultLimit = 10, ResultOffset = 2, Language = "en" };
        WikiSearchResponse response = searcher.Search(topic, searchSettings);

        return response.Query.SearchResults[1].Preview; // the first response seems related to the category, not a response.
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