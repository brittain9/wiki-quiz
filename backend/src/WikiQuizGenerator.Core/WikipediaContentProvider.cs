using System.Web;
using System.Text.Json;
using WikiQuizGenerator.Core.Models;
using WikiQuizGenerator.Core;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using WikiQuizGenerator.Core.Interfaces;

public class WikipediaContentProvider : IWikipediaContentProvider
{
    private HttpClient _client;
    private readonly ILogger<WikipediaContentProvider> _logger;

    public string ApiEndpoint
    { 
        get { return $"https://{Language.GetWikipediaLanguageCode()}.wikipedia.org/w/api.php"; } 
    }
    public Languages Language { get; set; }

    public string QueryApiEndpoint 
    {
        get { return $"{ApiEndpoint}?action=query&format=json&prop=extracts|info|links|categories&redirects=1&inprop=url|displaytitle&pllimit=100&titles="; } // + the query topic
    }

    public WikipediaContentProvider(ILogger<WikipediaContentProvider> logger)
    {
        _logger = logger;
        _client = new HttpClient();
        Language = Languages.English;
    }

    /// <summary>
    /// Fetches the content of a Wikipedia article based on the given topic.
    /// </summary>
    /// <param name="topic">The topic to search for on Wikipedia.</param>
    /// <returns>A WikipediaArticle object containing the article information.</returns>
    public async Task<WikipediaPage> GetWikipediaPage(string topic, Languages language)
    {
        if (Language != language) 
            Language = language;

        var query = HttpUtility.UrlEncode(topic);

        Stopwatch sw = Stopwatch.StartNew();
        string exactTitle = await GetWikipediaExactTitle(query);
        sw.Stop();
        _logger.LogInformation($"Got exact article name '{exactTitle}' from user entered topic '{topic}' in {sw.ElapsedMilliseconds} milliseconds");

        var exactQuery = HttpUtility.UrlEncode(exactTitle);
        var url = QueryApiEndpoint + exactQuery;

        sw.Restart();
        try
        {
            var response = await _client.GetStringAsync(url);
            var jsonDoc = JsonDocument.Parse(response);
            var pages = jsonDoc.RootElement.GetProperty("query").GetProperty("pages");

            // Get the first valid page of our query
            foreach (var page in pages.EnumerateObject())
            {
                var wikiPage = new WikipediaPage
                {
                    // TODO: Fix these null reference assignment warnings
                    Id = page.Value.GetProperty("pageid").GetInt32(),
                    Title = page.Value.GetProperty("title").GetString(),
                    Language = page.Value.GetProperty("pagelanguage").GetString(),
                    Extract = RemoveFormatting(page.Value.GetProperty("extract").GetString()),
                    LastModified = DateTime.Parse(page.Value.GetProperty("touched").GetString()),
                    Url = page.Value.GetProperty("fullurl").GetString(),
                    Length = page.Value.GetProperty("length").GetInt32()
                };

                // TODO: Implement the code for this...
                //if (page.Value.TryGetProperty("links", out var links))
                //{
                //    foreach (var link in links.EnumerateArray())
                //    {
                //        // Could do every other link to reduce size
                //        wikiPage.Links.Add(link.GetProperty("title").GetString());
                //    }
                //}

                //if (page.Value.TryGetProperty("categories", out var categories))
                //{
                //    foreach (var category in categories.EnumerateArray())
                //    {
                //        wikiPage.Categories.Add(category.GetProperty("title").GetString());
                //    }
                //}

                sw.Stop(); // this takes 100-500 ms
                _logger.LogInformation($"Found Wikipedia page '{wikiPage.Title}' in {sw.ElapsedMilliseconds} milliseconds");
                return wikiPage;
            }

            return null; // idk why this would ever be hit, need to refactor
        }
        catch (Exception ex)
        {
            sw.Reset();
            _logger.LogError($"An error occurred: {ex.Message}");
            throw;
        }
    }

    public async Task<string> GetWikipediaExactTitle(string query)
    {
        string searchUrl = $"{ApiEndpoint}?action=opensearch&search={query}&limit=1&format=json";
        var searchResponse = await _client.GetStringAsync(searchUrl);
        var searchResults = JsonDocument.Parse(searchResponse);

        if (searchResults.RootElement.GetArrayLength() < 2 || !searchResults.RootElement[1].EnumerateArray().MoveNext())
        {
            throw new Exception("No search results found.");
        }

        return searchResults.RootElement[1][0].GetString();
    }

    public static string RemoveFormatting(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        // Remove all HTML tags
        input = Regex.Replace(input, @"<[^>]+>", string.Empty);

        // Remove extra whitespace
        input = Regex.Replace(input, @"\s+", " ");

        // Decode HTML entities
        input = System.Net.WebUtility.HtmlDecode(input);

        return input.Trim();
    }
}