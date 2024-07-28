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
    private readonly Kernel _kernel;

    public SemanticKernelQuestionGenerator(Kernel kernel)
    {
        _kernel = kernel;
    }

    /// <summary>
    /// Generates a list of multiple-choice questions based on input text using an AI model.
    /// </summary>
    /// <param name="page">The Wikipedia page to base questions on.</param>
    /// <param name="numQuestions">The number of questions to generate.</param>
    /// <param name="extractSubstringLength">Optional parameter to limit input text length (default 500 characters).</param>
    /// <param name="language">The language for the quiz (default English).</param>
    /// <returns>A Question Response object containing the questions and metadata.</returns>
    public async Task<QuestionResponse> GenerateQuestionsAsync(WikipediaPage page, int numQuestions, int extractSubstringLength, string language)
    {
        var _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

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

        chatMessages.AddUserMessage("Generate and respond only with the questions: ");

        var response = new ChatMessageContent();
        var questions = new List<Question>();
        var generationAttempts = 0;

        var timer = new Stopwatch(); // to get the response time
        timer.Start();
        do
        {
            // Get the AI response
            var result = await _chatCompletionService.GetChatMessageContentsAsync(chatMessages);
            response = result.LastOrDefault();

            if (response != null) // Try to extract questions from the response
                questions = ExtractQuestionsFromResult(response.Content);

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
