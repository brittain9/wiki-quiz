using Microsoft.AspNetCore.Mvc;
using WikiQuizGenerator.Core.Interfaces;


/// <summary>
/// Controller for generating quizzes based on Wikipedia content.
/// </summary>
[ApiController]
[Route("[controller]")]
public class QuizController : ControllerBase
{
    private readonly IQuestionGenerator _questionGenerator;
    private readonly IQuizGenerator _quizGenerator;

    public QuizController(IQuestionGenerator questionGenerator, IQuizGenerator quizGenerator)
    {
        _questionGenerator = questionGenerator;
        _quizGenerator = quizGenerator;
    }

    /// <summary>
    /// Generates questions based on a specified Wikipedia topic.
    /// </summary>
    /// <param name="topic">The Wikipedia topic to generate questions from.</param>
    /// <param name="numberOfQuestions">Number of questions to generate (1-35, default 10).</param>
    /// <param name="textSubstringLength">Length of text substring to use (default 500). Longer substring costs more but generates higher quality and more specific questions.</param>
    /// <returns>A JSON list of generated quiz questions.</returns>
    /// <response code="200">Returns the list of generated questions</response>
    /// <response code="400">If the topic is not found or invalid</response>
    [HttpGet("GenerateQuestions")]
    public async Task<IActionResult> GenerateQuestions(string topic, [FromQuery] int numberOfQuestions = 10, [FromQuery] int textSubstringLength = 500)
    {
        var wikiPage = await WikipediaContent.GetWikipediaPage(topic);
        var questions = await _questionGenerator.GenerateQuestionsAsync(wikiPage, numberOfQuestions, textSubstringLength);
        return Ok(questions);
    }

    /// <summary>
    /// Generates a basic quiz based only on the specified Wikipedia topic.
    /// </summary>
    /// <param name="topic">The Wikipedia topic to generate questions from.</param>
    /// <returns>A JSON object representing the quiz.</returns>
    /// <response code="200">Returns the quiz</response>
    /// <response code="400">If the topic is not found or invalid</response>
    [HttpGet("GenerateBasicQuiz")]
    public async Task<IActionResult> GenerateBasicQuiz(string topic, string language = "en")
    {
        var quiz = await _quizGenerator.GeneratorBasicQuizAsync(topic, language);
        if (quiz == null) return BadRequest($"Invalid topic: {topic}");
        Console.WriteLine(quiz.QuestionResponses[0].GetTotalTokens());
        return Ok(quiz);
    }
}