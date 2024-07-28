using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Text.Json;
using System.Collections.Generic;

using WikiQuizGenerator.Core.Models;
using WikiQuizGenerator.Core;

public class WikipediaContent
{
    /// <summary>
    /// Fetches the content of a Wikipedia article based on the given topic.
    /// </summary>
    /// <param name="topic">The topic to search for on Wikipedia.</param>
    /// <returns>A WikipediaArticle object containing the article information.</returns>
    public static async Task<WikipediaPage> GetWikipediaPage(string topic, string language = "en")
    {
        HttpClient client = new HttpClient();

        string API_ENDPOINT = $"https://{language}.wikipedia.org/w/api.php";


        var query = HttpUtility.UrlEncode(topic);

        // Construct the API URL with the specified language, get the extract, general info, links, and categories
        var url = $"{API_ENDPOINT}?action=query&format=json&prop=extracts|info|links|categories&redirects=1&inprop=url|displaytitle&pllimit=100&titles={query}";

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

                return wikiPage;
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return null;
        }
    }
}