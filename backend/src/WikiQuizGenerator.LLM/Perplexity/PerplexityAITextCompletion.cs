using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
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

public class PerplexityAIChatCompletion : IChatCompletionService
{
    private readonly string _apiKey;
    private readonly string _apiEndpoint;
    // The llama models don't like my current prompt... they dont generate only the json data causing errors.
    private readonly string _model; // llama-3-sonar-small-32k-chat, llama-3-70b-instruct, **mixtral-8x7b-instruct.
    private readonly HttpClient _httpClient;
    private readonly ILogger<PerplexityAIChatCompletion> _logger;

    public IReadOnlyDictionary<string, object?> Attributes { get; }
    public PerplexityAIChatCompletion(string apiKey, 
        string model, 
        string apiEndpoint = "https://api.perplexity.ai/chat/completions", 
        HttpClient? httpClient = null,
        ILogger<PerplexityAIChatCompletion> logger = null)
    {
        _apiKey = apiKey;
        _model = model;
        _apiEndpoint = apiEndpoint;
        _httpClient = httpClient ?? new HttpClient();
        _logger = logger ?? NullLogger<PerplexityAIChatCompletion>.Instance;

        Attributes = new Dictionary<string, object?>()
        {
            { "ModelId", "perplexity-" + _model },
            {"Endpoint", _apiEndpoint}
        };

        _logger.LogInformation("PerplexityAIChatCompletion initialized with model {Model} and endpoint {Endpoint}", _model, _apiEndpoint);
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
