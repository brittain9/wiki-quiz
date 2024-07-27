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

public class SemanticKernelQuestionGenerator : IQuestionGenerator
{
    private readonly IChatCompletionService _chatCompletionService; // so I can use message history

    // We create the kernel in the service extension method and inject it
    public SemanticKernelQuestionGenerator(IChatCompletionService chatCompletionService)
    {
        _chatCompletionService = chatCompletionService;

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
    public async Task<QuestionResponse> GenerateQuestionsAsync(WikipediaPage page, int numQuestions, int extractSubstringLength, string language)
    {
        // Shorten the extract the user defined length; default 500
        var shortenedText = page.Extract.Length > extractSubstringLength ? 
            page.Extract.Substring(0, extractSubstringLength) : page.Extract;

        // Limit the number of questions
        if (numQuestions < 0) numQuestions = 1;
        if (numQuestions > 35) numQuestions = 35;

        ChatHistory chatMessages = new ChatHistory();

        // change to use the semantic kernel prompt template later.
        chatMessages.AddSystemMessage($"""
You are an expert quiz creator. Your task is to create a quiz based on the given content. The quiz should be engaging, informative, and avoid trivial details like specific dates or names of lesser-known individuals.

Content: {shortenedText}

Number of questions: {numQuestions}

Language: {language}

Instructions:
1. Create {numQuestions} multiple-choice questions based on the provided content.
2. Each question should be independent and not require knowledge from other questions.
3. Focus on key concepts, interesting facts, and important ideas from the content.
4. Avoid questions about specific dates or names of people who are not well-known.
5. For each question, provide four options, with only one correct answer.
6. Output each question in an array in JSON format, following this structure:

    "Text": "Question text in {language}",
    "Options": [
    "Option 1 in {language}",
    "Option 2 in {language}",
    "Option 3 in {language}",
    "Option 4 in {language}"
    ],
    "CorrectAnswerIndex": number

7. Ensure that all text content (question text and options) is in the specified language ({language}).
8. Keep the JSON keys ("Text", "Options", "CorrectAnswerIndex") in English.
9. Ensure that the JSON is valid and follows the structure provided above.

Please generate the quiz questions in {language} while maintaining the JSON structure as specified.
""");


        chatMessages.AddUserMessage("Generate the question JSON now: ");

        var timer = new Stopwatch();
        timer.Start();

        var response = new ChatMessageContent();
        var questions = new List<Question>();

        do
        {
            // Get the AI response
            var result = await _chatCompletionService.GetChatMessageContentsAsync(chatMessages);
            response = result.LastOrDefault();

            if (response != null)
            {
                // Try to extract questions from the response
                questions = ExtractQuestionsFromResult(response.Content);
            }

            // If no valid questions were extracted, add a user message to clarify. Will this cause errors?
            if (questions.Count == 0)
            {
                chatMessages.AddUserMessage("The response was not in the correct format. Please provide the questions in a valid JSON format as specified earlier.");
            }

        } while (questions.Count == 0);

        timer.Stop();

        QuestionResponse questionResponse = new()
        {
            ResponseTopic = page.Title,
            TopicUrl = page.Url,
            AIResponseTime = timer.ElapsedMilliseconds,
            Questions = ExtractQuestionsFromResult(response.Content),
            ModelName = _chatCompletionService.GetModelId() ?? "NA"
        };

        if (response.Metadata.TryGetValue("Usage", out object? usageObj) && (usageObj is CompletionsUsage usage ))
        {
            questionResponse.PromptTokenUsage = usage.PromptTokens;
            questionResponse.CompletionTokenUsage = usage.CompletionTokens;
        } 
        else if (response.Metadata.TryGetValue("Usage", out object? PerUsageObj) && PerUsageObj is PerplexityUsage perUsage)
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
