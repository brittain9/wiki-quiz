using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Core.Interfaces;
public interface IWikipediaPageRepository
{
    Task<WikipediaPage> GetByIdAsync(int id);
    Task<IEnumerable<WikipediaPage>> GetAllAsync();
    Task<WikipediaPage> AddAsync(WikipediaPage wikipediaPage);
    Task UpdateAsync(WikipediaPage wikipediaPage);
    Task DeleteAsync(int id);
    Task<WikipediaPage> GetByTitleAsync(string title);
}