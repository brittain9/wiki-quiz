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
    private readonly IWikipediaPageRepository _pageRepository;
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

    public WikipediaContentProvider(IWikipediaPageRepository wikipediaPageRepository, ILogger<WikipediaContentProvider> logger)
    {
        _pageRepository = wikipediaPageRepository;
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
        _logger.LogTrace($"Getting wikipedia page content on topic '{topic}' in '{language.GetWikipediaLanguageCode()}'.");

        if (Language != language) 
            Language = language;

        var query = HttpUtility.UrlEncode(topic);

        Stopwatch sw = Stopwatch.StartNew();

        // Get the exact wikipedia page title using the wikipedia api search
        var exactTitle = await GetWikipediaExactTitle(query);
        if (string.IsNullOrEmpty(exactTitle))
        {
            throw new ArgumentException($"No Wikipedia page found for the given query.", nameof(query));
        }

        _logger.LogInformation($"Got exact article name '{exactTitle}' from user entered topic '{topic}' in {sw.ElapsedMilliseconds} milliseconds.");
        sw.Restart();

        // Check if we already have this page.
        if (await _pageRepository.ExistsByTitleAsync(exactTitle, language))
        {
            _logger.LogInformation($"Got page '{exactTitle}' in language '{language.GetWikipediaLanguageCode()}' from the database in {sw.ElapsedMilliseconds} milliseconds");
            return await _pageRepository.GetByTitleAsync(exactTitle, language);
        }

        var exactQuery = HttpUtility.UrlEncode(exactTitle);
        var url = QueryApiEndpoint + exactQuery;

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
                    WikipediaId = page.Value.GetProperty("pageid").GetInt32(),
                    Title = page.Value.GetProperty("title").GetString(),
                    Language = page.Value.GetProperty("pagelanguage").GetString(),
                    Extract = RemoveFormatting(page.Value.GetProperty("extract").GetString()),
                    LastModified = DateTime.Parse(page.Value.GetProperty("touched").GetString()).ToUniversalTime(),
                    Url = page.Value.GetProperty("fullurl").GetString(),
                    Length = page.Value.GetProperty("length").GetInt32(),
                    Links = new List<string>(),
                    Categories = new List<WikipediaCategory>()
                };

                if (page.Value.TryGetProperty("links", out var links))
                {
                    foreach (var link in links.EnumerateArray())
                    {
                        wikiPage.Links.Add(link.GetProperty("title").GetString()); // just add the link title string to the links list
                    }
                }

                if (page.Value.TryGetProperty("categories", out var categories))
                {
                    foreach (var category in categories.EnumerateArray())
                    {
                        var categoryName = category.GetProperty("title").GetString();

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

                sw.Stop();
                wikiPage = await _pageRepository.AddAsync(wikiPage);
                _logger.LogInformation($"Added Wikipedia page '{wikiPage.Title}' with language '{language.GetWikipediaLanguageCode()}' to database in {sw.ElapsedMilliseconds} milliseconds.");
                return wikiPage;
            }

            return null;
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
        _logger.LogTrace($"Getting exact wikipedia title from user topic '{query}'.");

        string searchUrl = $"{ApiEndpoint}?action=opensearch&search={query}&limit=1&format=json";
        try
        {
            var searchResponse = await _client.GetStringAsync(searchUrl); // the wikipedia api may not like vpns
            var searchResults = JsonDocument.Parse(searchResponse);

            if (searchResults.RootElement.GetArrayLength() < 2 || !searchResults.RootElement[1].EnumerateArray().MoveNext())
            {
                return string.Empty;
            }

            return searchResults.RootElement[1][0].GetString();
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Request error: {e.Message}");
        }
        return string.Empty;
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