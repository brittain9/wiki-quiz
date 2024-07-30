using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Text.Json;
using System.Collections.Generic;

using WikiQuizGenerator.Core.Models;
using WikiQuizGenerator.Core;
using System.Diagnostics;

public static class WikipediaContent
{
    private static HttpClient _client = new HttpClient(); // maybe use dependency injection later.
    public static string ApiEndpoint{ get { return $"https://{Language}.wikipedia.org/w/api.php"; } }
    public static string Language { get; set; } = "en"; // I need to add an Enum or dictionary for this

    /// <summary>
    /// Fetches the content of a Wikipedia article based on the given topic.
    /// </summary>
    /// <param name="topic">The topic to search for on Wikipedia.</param>
    /// <returns>A WikipediaArticle object containing the article information.</returns>
    public static async Task<WikipediaPage> GetWikipediaPage(string topic, string language = "en")
    {
        HttpClient client = new HttpClient();

        if (Language != language) Language = language; // this doesn't check for errors at all

        var query = HttpUtility.UrlEncode(topic);

        Stopwatch timer = new Stopwatch();
        timer.Start();
        string exactTitle = await GetWikipediaExactTitle(query);
        timer.Stop();
        Console.WriteLine($"Got exact article name '{exactTitle}' from user entered topic '{topic}' in {timer.ElapsedMilliseconds}");

        var exactQuery = HttpUtility.UrlEncode(exactTitle);

        timer.Restart();
        // Construct the API URL with the specified language, get the extract, general info, links, and categories
        var url = $"{ApiEndpoint}?action=query&format=json&prop=extracts|info|links|categories&redirects=1&inprop=url|displaytitle&pllimit=100&titles={exactQuery}";

        // This code is ugly but it works for now.
        try
        {
            var response = await client.GetStringAsync(url);
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
                    Extract = Utility.RemoveFormatting(page.Value.GetProperty("extract").GetString()),
                    LastModified = DateTime.Parse(page.Value.GetProperty("touched").GetString()),
                    Url = page.Value.GetProperty("fullurl").GetString(),
                    Length = page.Value.GetProperty("length").GetInt32()
                };


                if (page.Value.TryGetProperty("links", out var links))
                {
                    foreach (var link in links.EnumerateArray())
                    {
                        // Could do every other link to reduce size
                        wikiPage.Links.Add(link.GetProperty("title").GetString());
                    }
                }

                if (page.Value.TryGetProperty("categories", out var categories))
                {
                    foreach (var category in categories.EnumerateArray())
                    {
                        wikiPage.Categories.Add(category.GetProperty("title").GetString());
                    }
                }

                timer.Stop();
                Console.WriteLine($"Found Wikipedia page '{wikiPage.Title}' in {timer.ElapsedMilliseconds} milliseconds");
                return wikiPage;
            }

            return null; // idk why this would ever be hit, need to refactor
        }
        catch (Exception ex)
        {
            timer.Reset();
            Console.WriteLine($"An error occurred: {ex.Message}");
            throw;
        }
    }

    public static async Task<string> GetWikipediaExactTitle(string query)
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
}