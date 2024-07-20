using Microsoft.AspNetCore.Mvc;
using WikiQuizGenerator.Core.Interfaces;

[ApiController]
[Route("[controller]")]
public class QuizController : ControllerBase
{
    private readonly IQuizGenerator _quizGenerator;

    public QuizController(IQuizGenerator quizGenerator)
    {
        _quizGenerator = quizGenerator;
    }

    [HttpGet("generate")]
    public async Task<IActionResult> GenerateQuiz(string topic, [FromQuery] int numberOfQuestions = 2)
    {
        var wikiPage = await WikipediaContent.GetWikipediaPage(topic);
        var questions = await _quizGenerator.GenerateQuizQuestionsAsync(wikiPage.Extract, numberOfQuestions);
        return Ok(questions);
    }
}