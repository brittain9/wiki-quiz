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
    public async Task<IActionResult> GenerateQuiz([FromQuery] int numberOfQuestions = 5)
    {
        var articleContent = await _wikipediaRepository.GetRandomArticleContentAsync();
        var questions = await _quizGenerator.GenerateQuizQuestionsAsync(articleContent, numberOfQuestions);
        return Ok(questions);
    }

    [HttpGet("test")]
    public async Task<IActionResult> TestQuery()
    {
        var articleContent = await _wikipediaRepository.GetRandomArticleContentAsync();
        var response = await _quizGenerator.TestQuery(articleContent);
        return Ok(response);
    }
}