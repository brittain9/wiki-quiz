using WikiQuizGenerator.Core.Services;
using WikiQuizGenerator.Core.Utilities;

namespace WikiQuizGenerator.Core.Interfaces;

public interface IWikipediaContentService
{
    Task<WikipediaContentResult> GetWikipediaContentAsync(string topic, Languages language, int extractLength, CancellationToken cancellationToken);
    Task<string> GetWikipediaExactTitle(string query);
}
