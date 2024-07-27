using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Services;
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
    // The llama models are causing a Bad Request response from the perplexity api. Will look into it
    private readonly string _model; // llama-3-sonar-large-32k-online, llama-3-sonar-large-32k-chat, llama-3-70b-instruct, **mixtral-8x7b-instruct.
    private readonly HttpClient _httpClient;

    public IReadOnlyDictionary<string, object?> Attributes { get; }
    public PerplexityAITextCompletion(string apiKey, string model = "mixtral-8x7b-instruct" , string apiEndpoint = "https://api.perplexity.ai/chat/completions")
    {
        _apiKey = apiKey;
        _apiEndpoint = apiEndpoint;
        _httpClient = new HttpClient();
        _model = model;

        Attributes = new Dictionary<string, object?>()
        {
            { "ModelId", "perplexity-" + _model },
            {"Endpoint", _apiEndpoint}
        };
    }

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
