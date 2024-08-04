using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Core.Interfaces;

public interface IWikipediaPageRepository
{
    Task<WikipediaPage> GetByIdAsync(int id);
    Task<IEnumerable<WikipediaPage>> GetAllAsync();
    Task<WikipediaPage> AddAsync(WikipediaPage wikipediaPage);
    Task<bool> DeleteAsync(int id);

    Task<WikipediaPage> GetByTitleAsync(string title, Languages language);
    Task<bool> ExistsByTitleAsync(string title, Languages language);
    Task<WikipediaPage> GetByWikipediaIdAsync(int wikipediaId, Languages language);

    Task<IEnumerable<WikipediaPage>> GetByLanguageAsync(string language);
    Task<IEnumerable<WikipediaPage>> GetByCategoryAsync(string categoryName);

    Task DeleteEmptyCategoriesAsync();
}