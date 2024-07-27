using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class PerplexityAITextCompletion : IChatCompletionService
{
    private readonly string _apiKey;
    private readonly string _apiEndpoint;
    private readonly string _model;
    private readonly HttpClient _httpClient;

    public PerplexityAITextCompletion(string apiKey, string model = "mixtral-8x7b-instruct" , string apiEndpoint = "https://api.perplexity.ai/chat/completions")
    {
        _apiKey = apiKey;
        _apiEndpoint = apiEndpoint;
        _httpClient = new HttpClient();
        _model = model;
    }

    public IReadOnlyDictionary<string, object?> Attributes => new Dictionary<string, object?>();

    public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
    {
        var messages = new List<object>();
        foreach (var message in chatHistory)
        {
            messages.Add(new
            {
                role = message.Role.ToString().ToLower(),
                content = message.Content
            });
        }

        var requestBody = new
        {
            model = _model,
            messages = messages
        };

        var request = new HttpRequestMessage(HttpMethod.Post, _apiEndpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
        };
        request.Headers.Add("Authorization", $"Bearer {_apiKey}");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        var perplexityResponse = JsonSerializer.Deserialize<PerplexityResponse>(responseBody);

        var content = perplexityResponse.Choices[0].Message.Content;

        var metadata = new Dictionary<string, object?>
        {
            { "Id", perplexityResponse.Id },
            { "Model", perplexityResponse.Model },
            { "Created", perplexityResponse.Created },
            { "Usage", perplexityResponse.Usage },
            { "FinishReason", perplexityResponse.Choices[0].FinishReason }
        };

        return new List<ChatMessageContent> 
        { 
            new ChatMessageContent(AuthorRole.Assistant, content, metadata: metadata) 
        };
    }

    public IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
