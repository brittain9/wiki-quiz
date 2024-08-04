using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Core.Interfaces;

public interface IWikipediaPageRepository
{
    Task<WikipediaPage> GetByIdAsync(int id);
    Task<IEnumerable<WikipediaPage>> GetAllAsync();
    Task<WikipediaPage> AddAsync(WikipediaPage wikipediaPage);
    Task<bool> DeleteAsync(int id);

    Task<WikipediaPage> GetByTitleAsync(string title);
    Task<bool> ExistsByTitleAsync(string title);

    Task<IEnumerable<WikipediaPage>> GetByLanguageAsync(string language);
    Task<IEnumerable<WikipediaPage>> GetByCategoryAsync(string categoryName);

    Task DeleteEmptyCategoriesAsync();
}