namespace WikiQuizGenerator.Core.Interfaces;

public interface IWikipediaRepository
{
    Task<string> GetRandomArticleContentAsync();
}