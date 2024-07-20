using Microsoft.AspNetCore.Mvc;
using WikiQuizGenerator.Core.Interfaces;

[ApiController]
[Route("[controller]")]
public class QuizController : ControllerBase
{
    private readonly IQuestionGenerator _questionGenerator;

    public QuizController(IQuestionGenerator questionGenerator)
    {
        _questionGenerator = questionGenerator;
    }

    [HttpGet("generate")]
    public async Task<IActionResult> GenerateQuiz(string topic, [FromQuery] int numberOfQuestions = 2, [FromQuery] int textSubstringLength = 500)
    {
        var wikiPage = await WikipediaContent.GetWikipediaPage(topic);
        var questions = await _questionGenerator.GenerateQuestionsAsync(wikiPage.Extract, numberOfQuestions, textSubstringLength);
        return Ok(questions);
    }
}