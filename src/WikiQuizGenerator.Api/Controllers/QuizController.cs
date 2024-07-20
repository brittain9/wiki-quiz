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
    public async Task<IActionResult> GenerateQuiz(string topic, [FromQuery] int numberOfQuestions = 5)
    {
        var wikiPage = await WikipediaContent.GetWikipediaPage(topic);
        var questions = await _quizGenerator.GenerateQuizQuestionsAsync(wikiPage.Extract, numberOfQuestions);
        return Ok(questions);
    }

    [HttpGet("test")]
    public async Task<IActionResult> TestQuery(string topic)
    {
        var wikiPage = await WikipediaContent.GetWikipediaPage(topic);

        string response;

        if (wikiPage != null)
            response = await _quizGenerator.TestQuery(wikiPage.Extract.Substring(0, 100));
        else
            response = "Wiki Page was null";

        return Ok(response);
    }
}