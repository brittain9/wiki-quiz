using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Core.Interfaces;

public interface IQuizGenerator
{
    Task<List<QuizQuestion>> GenerateQuizQuestionsAsync(string text, int numberOfQuestions);
    Task<string> TestQuery(string text);
}