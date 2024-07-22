using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using Azure.AI.OpenAI;
using Microsoft.SemanticKernel;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.LLM;

public class SemanticKernelQuestionGenerator : IQuestionGenerator
{
    private readonly Kernel _kernel;

    public SemanticKernelQuestionGenerator(Kernel kernel)
    {
        _kernel = kernel;
    }

    /// <summary>
    /// Generates a list of multiple-choice questions based on input text using an AI model.
    /// </summary>
    /// <param name="text">The full text to base questions on.</param>
    /// <param name="numberOfQuestions">The number of questions to generate.</param>
    /// <param name="reducedTextLength">Optional parameter to limit input text length (default 500 characters).</param>
    /// <returns>A list of Question objects.</returns>
    /// <remarks>
    /// This function balances between generating high-quality questions and managing token usage.
    /// It shortens the input text to reduce token consumption, constructs a detailed prompt,
    /// invokes an AI model, and processes the results.
    /// 
    /// Token usage notes:
    /// - Prompt tokens: ~320 tokens with text shortened to 500 chars, used whooping 8000 tokens for the full extract about C++
    /// Used 15000 input tokens for topic Planets! The question quality seems to improve with higher token usage.
    /// 
    /// - Completion tokens vary with number of questions:
    ///   ~125 for 2 questions, ~600 for 10 questions, ~1100 for 20 questions (using gpt-4-mini)
    ///   
    /// - GPT-4o mini costs 15 cents per 1M input tokens and 60 cents per 1M output tokens.
    /// </remarks>
    public async Task<QuestionResponse> GenerateQuestionsAsync(WikipediaPage page, int numQuestions, int extractSubstringLength = 500)
    {
        var text = page.Extract;
        var shortenedText= text.Length > extractSubstringLength ? text.Substring(0, extractSubstringLength) : text;

        if (numQuestions < 0) numQuestions = 1;
        if (numQuestions > 35) numQuestions = 35; // limit 35, ive only tested with 20 so far. Maybe do this in the api but this works for now

        var prompt = @"Generate {{$numberOfQuestions}} questions based on the following content. 
            Each question should be multiple-choice with 4 options. 
            Format the output as a JSON array of objects, where each object has the following properties:
            - 'Text': The question text (string)
            - 'Options': An array of 4 strings representing the possible answers
            - 'CorrectAnswerIndex': The zero-based index of the correct answer in the Options array (integer)

            Ensure that:
            1. Questions are diverse and cover different aspects of the given content.
            2. Questions are clear, concise, and unambiguous.
            3. Questions are fun and engaging.
            4. All options are plausible, with only one correct answer.
            5. The correct answer is not always in the same position.
            6. Questions test understanding rather than just memorization.
            7. Grammar and spelling are correct in both questions and options.

            Content to base questions on:
            {{$inputContent}}

            Output the JSON array of Question objects:";

        var function = _kernel.CreateFunctionFromPrompt(prompt);

        var result = await function.InvokeAsync(_kernel, new KernelArguments
        {
            ["inputContent"] = shortenedText,
            ["numberOfQuestions"] = numQuestions.ToString()
        });

        var jsonResult = result.GetValue<string>() ?? "[]";

        QuestionResponse questionResponse = new()
        {
            ResponseTopic = page.Title,
            TopicUrl = page.Url,
            Questions = ExtractQuestionsFromResult(jsonResult),
        };

        if (result.Metadata.TryGetValue("Usage", out object? usageObj) && usageObj is CompletionsUsage usage)
        {
            questionResponse.PromptTokenUsage = usage.PromptTokens;
            questionResponse.CompletionTokenUsage = usage.CompletionTokens;
        }

        return questionResponse;
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

    private string CleanJsonString(string input)
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
}
