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
        // var articleContent = _wikipediaRepository.GetArticleContent(topic);
        var articleContent = "test";
        var questions = await _quizGenerator.GenerateQuizQuestionsAsync(articleContent, numberOfQuestions);
        return Ok(questions);
    }

    [HttpGet("test")]
    public async Task<IActionResult> TestQuery(string topic)
    {
        // var articleContent = _wikipediaRepository.GetArticleContent(topic);
        var articleContent = "test";
        var response = await _quizGenerator.TestQuery(articleContent);
        return Ok(response);
    }
}