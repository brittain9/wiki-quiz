using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Core.Interfaces;

public interface IQuizGenerator
{
    Task<List<Question>> GenerateQuizQuestionsAsync(string text, int numberOfQuestions);
    Task<string> TestQuery(string text);
}