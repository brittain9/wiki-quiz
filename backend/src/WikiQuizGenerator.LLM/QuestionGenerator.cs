using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Azure.AI.OpenAI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
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

        var promptTemplatesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PromptTemplates"); // prompt templates are copied to the build directory in .csproj

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
    public async Task<AIResponse> GenerateQuestionsAsync(WikipediaPage page, string language, int numQuestions, int numOptions, int extractSubstringLength)
    {
        // Shorten the extract the user defined length; default 500
        var shortenedText = page.Extract.Length > extractSubstringLength ? 
            page.Extract.Substring(0, extractSubstringLength) : page.Extract;

        // Limit the number of questions and options
        numQuestions = Math.Clamp(numQuestions, 1, 35);
        numOptions = Math.Clamp(numOptions, 2, 5);

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
                ["numOptions"] = numOptions,
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

        AIMetadata aiMetadata = new()
        {
            ModelName = _kernel.GetRequiredService<IChatCompletionService>().GetModelId() ?? "NA",
        };

        if (result.Metadata.TryGetValue("Usage", out object? usageObj) && (usageObj is CompletionsUsage usage ))
        {
            aiMetadata.PromptTokenUsage = usage.PromptTokens;
            aiMetadata.CompletionTokenUsage = usage.CompletionTokens;
        } 
        else if (result.Metadata.TryGetValue("Usage", out object? PerUsageObj) && PerUsageObj is PerplexityUsage perUsage)
        {
            aiMetadata.PromptTokenUsage = perUsage.PromptTokens;
            aiMetadata.CompletionTokenUsage = perUsage.CompletionTokens;
        }

        AIResponse aiResponse = new()
        {
            ResponseTime = timer.ElapsedMilliseconds,
            Questions = questions,
            WikipediaPage = page,
            AIMetadata = aiMetadata
        };

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
                    CorrectOptionNumber = jsonQuestion.GetProperty("CorrectAnswerOption").GetInt32()
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
            Debug.WriteLine($"Error parsing JSON: {ex.Message}");
            Debug.WriteLine($"Cleaned JSON string: {cleanedResult}");
            return new List<Question>();
        }
    }
}