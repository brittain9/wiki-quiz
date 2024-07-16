using WikiQuizGenerator.Core.Interfaces;

namespace WikiQuizGenerator.Data;
public class WikipediaRepository : IWikipediaRepository
{
    // Implement database access logic here
    public async Task<string> GetRandomArticleContentAsync()
    {
        // Placeholder implementation
        return @"Sample Wikipedia article content.";
    }
}