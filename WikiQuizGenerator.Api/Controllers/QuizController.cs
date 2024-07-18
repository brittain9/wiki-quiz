using Microsoft.AspNetCore.Mvc;
using WikiQuizGenerator.Core.Interfaces;

[ApiController]
[Route("[controller]")]
public class QuizController : ControllerBase
{
    private readonly IQuizGenerator _quizGenerator;
    private readonly IWikipediaRepository _wikipediaRepository;

    public QuizController(IQuizGenerator quizGenerator, IWikipediaRepository wikipediaRepository)
    {
        _quizGenerator = quizGenerator;
        _wikipediaRepository = wikipediaRepository;
    }

    [HttpGet("generate")]
    public async Task<IActionResult> GenerateQuiz(string topic, [FromQuery] int numberOfQuestions = 5)
    {
        var articleContent = _wikipediaRepository.GetArticleContent(topic);
        var questions = await _quizGenerator.GenerateQuizQuestionsAsync(articleContent, numberOfQuestions);
        return Ok(questions);
    }

    [HttpGet("test")]
    public async Task<IActionResult> TestQuery(string topic)
    {
        var articleContent = _wikipediaRepository.GetArticleContent(topic);
        var response = await _quizGenerator.TestQuery(articleContent);
        return Ok(response);
    }
}