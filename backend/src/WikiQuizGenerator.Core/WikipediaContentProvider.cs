using System.Web;
using System.Text.Json;
using WikiQuizGenerator.Core.Models;
using WikiQuizGenerator.Core;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using WikiQuizGenerator.Core.Interfaces;

namespace WikiQuizGenerator.Core;

public class WikipediaContentProvider : IWikipediaContentProvider, IDisposable
{
    private readonly HttpClient _client;
    private readonly ILogger<WikipediaContentProvider> _logger;

    public Languages Language { get; set; }

    public string ApiEndpoint => $"https://{Language.GetWikipediaLanguageCode()}.wikipedia.org/w/api.php";

    public string QueryApiEndpoint => $"{ApiEndpoint}?action=query&format=json&prop=extracts|info|links|categories&redirects=1&inprop=url|displaytitle&pllimit=100&titles=";

    public WikipediaContentProvider(ILogger<WikipediaContentProvider> logger)
    {
        _logger = logger;
        _client = new HttpClient();
        Language = Languages.English;
    }

    /// <summary>
    /// Fetches the content of a Wikipedia article based on the given topic.
    /// This method no longer stores pages in database - it fetches fresh data each time.
    /// </summary>
    /// <param name="topic">The topic to search for on Wikipedia.</param>
    /// <param name="language">The language to fetch the content in.</param>
    /// <param name="cancellationToken">Cancellation token for the request.</param>
    /// <returns>A WikipediaPage object containing the article information.</returns>
    public async Task<WikipediaPage> GetWikipediaPage(string topic, Languages language, CancellationToken cancellationToken)
    {
        _logger.LogTrace("Getting wikipedia page content on topic '{Topic}' in '{Language}'.", topic, language.GetWikipediaLanguageCode());

        if (Language != language) 
            Language = language;

        var query = HttpUtility.UrlEncode(topic);
        var sw = Stopwatch.StartNew();

        // Get the exact wikipedia page title using the wikipedia api search
        var exactTitle = await GetWikipediaExactTitle(query);
        if (string.IsNullOrEmpty(exactTitle))
        {
            throw new ArgumentException($"No Wikipedia page found for the given query.", nameof(topic));
        }

        _logger.LogInformation("Got exact article name '{ExactTitle}' from user entered topic '{Topic}' in {ElapsedMilliseconds} milliseconds.", 
            exactTitle, topic, sw.ElapsedMilliseconds);
        
        sw.Restart();

        var exactQuery = HttpUtility.UrlEncode(exactTitle);
        var url = QueryApiEndpoint + exactQuery;

        try
        {
            var response = await _client.GetStringAsync(url, cancellationToken);
            var jsonDoc = JsonDocument.Parse(response);
            var pages = jsonDoc.RootElement.GetProperty("query").GetProperty("pages");

            // Get the first valid page of our query
            foreach (var page in pages.EnumerateObject())
            {
                var wikiPage = new WikipediaPage
                {
                    WikipediaId = page.Value.GetProperty("pageid").GetInt32(),
                    Title = page.Value.GetProperty("title").GetString() ?? string.Empty,
                    Language = page.Value.GetProperty("pagelanguage").GetString() ?? language.GetWikipediaLanguageCode(),
                    Extract = RemoveFormatting(page.Value.GetProperty("extract").GetString()),
                    LastModified = DateTime.Parse(page.Value.GetProperty("touched").GetString()).ToUniversalTime(),
                    Url = page.Value.GetProperty("fullurl").GetString() ?? string.Empty,
                    Length = page.Value.GetProperty("length").GetInt32(),
                    Links = new List<string>(),
                    Categories = new List<WikipediaCategory>()
                };

                if (page.Value.TryGetProperty("links", out var links))
                {
                    foreach (var link in links.EnumerateArray())
                    {
                        var linkTitle = link.GetProperty("title").GetString();
                        if (!string.IsNullOrEmpty(linkTitle))
                        {
                            wikiPage.Links.Add(linkTitle);
                        }
                    }
                }

                if (page.Value.TryGetProperty("categories", out var categories))
                {
                    foreach (var category in categories.EnumerateArray())
                    {
                        var categoryName = category.GetProperty("title").GetString();

                        if (!string.IsNullOrEmpty(categoryName))
                        {
                            // Remove "Category:" prefix if present
                            if (categoryName.StartsWith("Category:"))
                            {
                                categoryName = categoryName.Substring("Category:".Length);
                            }
                            
                            wikiPage.Categories.Add(new WikipediaCategory
                            {
                                Name = categoryName
                            });
                        }
                    }
                }

                sw.Stop();
                _logger.LogInformation("Fetched Wikipedia page '{Title}' with language '{Language}' from API in {ElapsedMilliseconds} milliseconds.", 
                    wikiPage.Title, language.GetWikipediaLanguageCode(), sw.ElapsedMilliseconds);
                
                return wikiPage;
            }

            throw new InvalidOperationException($"No valid page found in Wikipedia API response for topic: {topic}");
        }
        catch (Exception ex) when (!(ex is ArgumentException || ex is InvalidOperationException))
        {
            _logger.LogError(ex, "An error occurred while fetching Wikipedia page for topic '{Topic}'", topic);
            throw new InvalidOperationException($"Failed to fetch Wikipedia page for topic: {topic}", ex);
        }
    }

    public async Task<string> GetWikipediaExactTitle(string query)
    {
        _logger.LogTrace("Getting exact wikipedia title from user topic '{Query}'.", query);

        string searchUrl = $"{ApiEndpoint}?action=opensearch&search={query}&limit=1&format=json";
        try
        {
            var searchResponse = await _client.GetStringAsync(searchUrl);
            var searchResults = JsonDocument.Parse(searchResponse);

            if (searchResults.RootElement.GetArrayLength() < 2 || !searchResults.RootElement[1].EnumerateArray().MoveNext())
            {
                return string.Empty;
            }

            return searchResults.RootElement[1][0].GetString() ?? string.Empty;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Request error while getting Wikipedia title for query '{Query}'", query);
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while getting Wikipedia title for query '{Query}'", query);
            return string.Empty;
        }
    }

    public static string RemoveFormatting(string? input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        // Remove all HTML tags
        input = Regex.Replace(input, @"<[^>]+>", string.Empty);

        // Remove extra whitespace
        input = Regex.Replace(input, @"\s+", " ");

        // Decode HTML entities
        input = System.Net.WebUtility.HtmlDecode(input);

        return input.Trim();
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}