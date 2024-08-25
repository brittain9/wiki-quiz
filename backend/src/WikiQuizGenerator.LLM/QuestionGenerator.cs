using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Services;
using WikiQuizGenerator.Core;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.LLM;

public class QuestionGenerator : IQuestionGenerator
{
    private readonly PromptManager _promptManager;
    private readonly ILogger<QuestionGenerator> _logger;
    private readonly Kernel _kernel;

    public QuestionGenerator(PromptManager promptManager, ILogger<QuestionGenerator> logger, AiServiceManager aiServiceManager)
    {
        // created by our factory so this will run each request
        _promptManager = promptManager;
        _logger = logger;
        
        // create the kernel with the specified ai service
        var kernelBuilder = Kernel.CreateBuilder();
        
        string modelId = aiServiceManager.SelectedService switch
        {
            AiService.OpenAi => AiServiceManager.OpenAiModelNames[aiServiceManager.SelectedOpenAiModel],
            AiService.Perplexity => AiServiceManager.PerplexityModelNames[aiServiceManager.SelectedPerplexityModel],
            _ => throw new ArgumentException("Invalid AI service selected.")
        };
        
        switch (aiServiceManager.SelectedService)
        {
            case AiService.OpenAi:
                if (AiServiceManager.IsOpenAiAvailable)
                    kernelBuilder.AddOpenAIChatCompletion(modelId, AiServiceManager.OpenAiApiKey!);
                break;
            case AiService.Perplexity:
                if (AiServiceManager.IsPerplexityAvailable)
                    kernelBuilder.AddPerplexityAIChatCompletion(modelId, AiServiceManager.PerplexityApiKey!);
                break;
            default:
                throw new ArgumentException("Invalid AI service selected.");
        }
        
        _kernel = kernelBuilder.Build();
    }

    public async Task<AIResponse> GenerateQuestionsAsync(WikipediaPage page, string content, Languages language, int numQuestions, int numOptions)
    {
        _logger.LogTrace($"Generating {numQuestions} questions on page '{page.Title}' in '{language.GetWikipediaLanguageCode()}' with {numOptions} options and {content.Length} content length");
        
        var quizFunction = _promptManager.GetPromptFunction("Default", language.GetWikipediaLanguageCode());
        
        var kernelArgs = new KernelArguments
        {
            ["text"] = content,
            ["numQuestions"] = numQuestions,
            ["numOptions"] = numOptions,
            ["language"] = language
        };
        
        Stopwatch sw = Stopwatch.StartNew();

        var questions = new List<Question>();
        FunctionResult result = null!;
        var generationAttempts = 0;;

        do
        {
            if (generationAttempts > 0)
                _logger.LogWarning($"Failed extracting questions from response on attempt {generationAttempts}.");

            result = await quizFunction.InvokeAsync(_kernel, kernelArgs);

            var jsonResult = result.GetValue<string>();

            if (!string.IsNullOrEmpty(jsonResult))
                questions = ExtractQuestionsFromResult(jsonResult);

            generationAttempts++;

        } while (questions.Count == 0 && generationAttempts < 3);

        sw.Stop();

        if (generationAttempts >= 3)
        {
            return null;
        }

        AIResponse aiResponse = new()
        {
            Questions = questions,
            WikipediaPageId = page.Id,
            WikipediaPage = page,
            ModelName = _kernel.GetRequiredService<IChatCompletionService>().GetModelId() ?? "NA",
            ResponseTime = sw.ElapsedMilliseconds,
        };

        if (result.Metadata.TryGetValue("Usage", out object? usageObj) && (usageObj is CompletionsUsage usage ))
        {
            aiResponse.PromptTokenUsage = usage.PromptTokens;
            aiResponse.CompletionTokenUsage = usage.CompletionTokens;
        } 
        else if (result.Metadata.TryGetValue("Usage", out object? PerUsageObj) && PerUsageObj is PerplexityUsage perUsage)
        {
            aiResponse.PromptTokenUsage = perUsage.PromptTokens;
            aiResponse.CompletionTokenUsage = perUsage.CompletionTokens;
        }

        return aiResponse;
    }

    public static string CleanJsonString(string input)
    {
        // Remove code fences
        var withoutCodeFences = Regex.Replace(input, @"^\s*```(?:json)?\s*|\s*```\s*$", "", RegexOptions.Multiline);

        // Remove extra brackets at the start and end
        var withoutExtraBrackets = Regex.Replace(withoutCodeFences, @"^\s*\[?\s*|\s*\]?\s*$", "");

        // Trim whitespace and ensure the string is wrapped in brackets
        var trimmed = withoutExtraBrackets.Trim();
        if (!trimmed.StartsWith("["))
            trimmed = "[" + trimmed;
        if (!trimmed.EndsWith("]"))
            trimmed = trimmed + "]";

        return trimmed;
    }

    private List<Question> ExtractQuestionsFromResult(string result)
    {
        _logger.LogTrace($"Extracting questions from result");

        // Remove code fences, extra brackets, and whitespace
        var cleanedResult = CleanJsonString(result);

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var jsonQuestions = JsonSerializer.Deserialize<List<JsonElement>>(cleanedResult, options);
            var questions = new List<Question>();

            foreach (var jsonQuestion in jsonQuestions)
            {
                var question = new Question
                {
                    Text = jsonQuestion.GetProperty("Text").GetString(),
                    Option1 = "",
                    Option2 = "",
                    CorrectOptionNumber = jsonQuestion.GetProperty("CorrectAnswerIndex").GetInt32() + 1 // 0 based index for the options array turned to the option number
                };

                var jsonOptions = jsonQuestion.GetProperty("Options").EnumerateArray();
                int optionCount = 0;
                foreach (var jsonOption in jsonOptions)
                {
                    optionCount++;
                    switch (optionCount)
                    {
                        case 1:
                            question.Option1 = jsonOption.GetString();
                            break;
                        case 2:
                            question.Option2 = jsonOption.GetString();
                            break;
                        case 3:
                            question.Option3 = jsonOption.GetString();
                            break;
                        case 4:
                            question.Option4 = jsonOption.GetString();
                            break;
                        case 5:
                            question.Option5 = jsonOption.GetString();
                            break;
                        default:
                            // Ignore additional options if more than 5
                            break;
                    }
                }

                questions.Add(question);
            }

            return questions;
        }
        catch (JsonException ex)
        {
            _logger.LogError($"Error parsing JSON: {ex.Message}");
            _logger.LogError($"Cleaned JSON string: {cleanedResult}");
            return new List<Question>();
        }
    }
}