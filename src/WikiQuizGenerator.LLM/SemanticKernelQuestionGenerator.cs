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

string prompt = @"You are an AI quiz generator. Your primary task is to create a JSON array of quiz questions based on this Wikipedia snippet: {{ $text }}

IMPORTANT: Your entire response must be valid JSON. Do not include any text before or after the JSON array.

Generate {{ $number_of_questions }} engaging questions. Format your output strictly as follows:

[
  {
    ""Text"": ""Question text here"",
    ""Options"": [""Option 1"", ""Option 2"", ""Option 3"", ""Option 4""],
    ""CorrectAnswerIndex"": 0
  },
  // More questions...
]

Guidelines for creating an engaging quiz:
1. Make questions standalone, not requiring the original text.
2. Use varied question types (multiple choice, scenarios, etc.).
3. Focus on fascinating insights, unexpected twists, and 'aha!' moments.
4. Challenge players to think creatively or apply knowledge in fun ways.
5. Ensure all 4 options are plausible, with entertaining wrong answers.
6. Vary difficulty naturally, including both easy and challenging questions.

Remember: Your entire output must be a valid JSON array of question objects. Do not include any explanations or additional text outside the JSON structure.";
        var function = _kernel.CreateFunctionFromPrompt(prompt);

        var result = await function.InvokeAsync(_kernel, new KernelArguments
        {
            ["text"] = shortenedText,
            ["number_of_questions"] = numQuestions.ToString()
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
