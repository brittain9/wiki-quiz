using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Azure.AI.OpenAI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Services;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.LLM;

public class QuestionGenerator : IQuestionGenerator
{
    private readonly Kernel _kernel;
    private readonly PromptManager _promptManager;

    public QuestionGenerator(Kernel kernel)
    {
        _kernel = kernel;

        // copy the prompt templates to the build directory in .csproj
        var promptTemplatesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PromptTemplates"); 

        _promptManager = new PromptManager(promptTemplatesPath);
    }

    /// <summary>
    /// Generates a list of multiple-choice questions based on input text using an AI model.
    /// </summary>
    /// <param name="page">The Wikipedia page to base questions on.</param>
    /// <param name="numQuestions">The number of questions to generate.</param>
    /// <param name="extractSubstringLength">Optional parameter to limit input text length (default 500 characters).</param>
    /// <param name="language">The language for the quiz (default English).</param>
    /// <returns>A Question Response object containing the questions and metadata.</returns>
    public async Task<QuestionResponse> GenerateQuestionsAsync(WikipediaPage page, string language, int numQuestions, int extractSubstringLength)
    {
        // Shorten the extract the user defined length; default 500
        var shortenedText = page.Extract.Length > extractSubstringLength ? 
            page.Extract.Substring(0, extractSubstringLength) : page.Extract;

        // Limit the number of questions
        numQuestions = Math.Clamp(numQuestions, 1, 35);

        var quizFunction = _promptManager.GetPromptFunction("Default", language);

        var timer = new Stopwatch(); // to get the response time
        timer.Start();

        var questions = new List<Question>();
        FunctionResult result = null!;
        var generationAttempts = 0;;

        do
        {
            result = await quizFunction.InvokeAsync(_kernel, new KernelArguments
            {
                ["text"] = shortenedText,
                ["numQuestions"] = numQuestions,
                ["language"] = language
            });

            var jsonResult = result.GetValue<string>();

            if (!string.IsNullOrEmpty(jsonResult))
                questions = ExtractQuestionsFromResult(jsonResult);

            generationAttempts++;

        } while (questions.Count == 0 && generationAttempts < 3);

        timer.Stop();

        if (generationAttempts >= 3)
        {
            return null;
        }

        QuestionResponse questionResponse = new()
        {
            ResponseTopic = page.Title,
            TopicUrl = page.Url,
            AIResponseTime = timer.ElapsedMilliseconds,
            Questions = questions,
            ModelName = _kernel.GetRequiredService<IChatCompletionService>().GetModelId() ?? "NA"
        };

        if (result.Metadata.TryGetValue("Usage", out object? usageObj) && (usageObj is CompletionsUsage usage ))
        {
            questionResponse.PromptTokenUsage = usage.PromptTokens;
            questionResponse.CompletionTokenUsage = usage.CompletionTokens;
        } 
        else if (result.Metadata.TryGetValue("Usage", out object? PerUsageObj) && PerUsageObj is PerplexityUsage perUsage)
        {
            questionResponse.PromptTokenUsage = perUsage.PromptTokens;
            questionResponse.CompletionTokenUsage = perUsage.CompletionTokens;
        }

        return questionResponse;
    }

    private List<Question> ExtractQuestionsFromResult(string result)
    {
        // Remove code fences, extra brackets, and whitespace
        var cleanedResult = Utility.CleanJsonString(result);

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };                

            var questions = JsonSerializer.Deserialize<List<Question>>(cleanedResult, options);

            return questions ?? new List<Question>();
        }
        catch (JsonException ex)
        {
            Debug.WriteLine($"Error parsing JSON: {ex.Message}");
            Debug.WriteLine($"Cleaned JSON string: {cleanedResult}");
            return new List<Question>();
        }
    }
}
