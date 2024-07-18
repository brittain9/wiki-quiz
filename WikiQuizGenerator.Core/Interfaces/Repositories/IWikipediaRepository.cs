using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Core.Interfaces;

public interface IWikipediaRepository
{
    string GetArticleContent(string topic);
    
    // TODO: figure out how to cache this based on the wikipedia api result
    Task<CachedWikipediaData> GetCachedArticleAsync(string title);
    Task CacheArticleAsync(CachedWikipediaData data);
}