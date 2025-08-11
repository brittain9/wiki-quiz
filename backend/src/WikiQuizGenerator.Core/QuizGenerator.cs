using Microsoft.Extensions.Logging;
using WikiQuizGenerator.Core.DomainObjects;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Utilities;

namespace WikiQuizGenerator.Core;

public class QuizGenerator(
        IQuestionGeneratorFactory questionGeneratorFactory,
        IWikipediaContentService wikipediaContentService,
        IAiServiceManager aiServiceManager,
        ILogger<QuizGenerator> logger,
        IQuizRepository quizRepository) : IQuizGenerator
{
    public async Task<Quiz> GenerateQuizAsync(
        string topic,
        Languages language,
        string aiService,
        string model,
        int numQuestions,
        int numOptions,
        int extractLength,
        Guid createdBy,
        CancellationToken cancellationToken)
    {
        logger.LogTrace($"Generating a quiz on '{topic}' in '{language.GetWikipediaLanguageCode()}' with {numQuestions} questions, {numOptions} options, and {extractLength} extract length.");

        var contentResult = await wikipediaContentService.GetWikipediaContentAsync(topic, language, extractLength, cancellationToken);

        var questionGenerator = questionGeneratorFactory.Create(aiServiceManager, aiService, model);

        var generationResult = await questionGenerator.GenerateQuestionsAsync(contentResult.ProcessedContent, language, numQuestions, numOptions, cancellationToken);
        
        if (generationResult == null || generationResult.Questions.Count == 0)
            throw new InvalidOperationException("Failed to generate quiz questions");

        // Log generation metrics for monitoring/cost tracking
        if (generationResult.InputTokenCount.HasValue && generationResult.OutputTokenCount.HasValue)
        {
            logger.LogInformation("Generated quiz with {QuestionCount} questions using {InputTokens} input tokens and {OutputTokens} output tokens in {ResponseTime}ms",
                generationResult.Questions.Count,
                generationResult.InputTokenCount.Value,
                generationResult.OutputTokenCount.Value,
                generationResult.ResponseTimeMs);
        }

        Quiz quiz = new Quiz()
        {
            Title = contentResult.WikipediaReference.Title,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy,
            Questions = generationResult.Questions,
            WikipediaReference = contentResult.WikipediaReference,
            InputTokenCount = generationResult.InputTokenCount,
            OutputTokenCount = generationResult.OutputTokenCount,
            ResponseTimeMs = (long?)generationResult.ResponseTimeMs,
            ModelId = generationResult.ModelId,
            EstimatedCostUsd = generationResult.EstimatedCostUsd
        };

        var result = await quizRepository.AddAsync(quiz);
        return result;
    }
}
