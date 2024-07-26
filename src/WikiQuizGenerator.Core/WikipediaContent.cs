using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Text.Json;
using System.Collections.Generic;

using WikiQuizGenerator.Core.Models;

public class WikipediaContent
{
    private static readonly HttpClient client = new HttpClient();
    private const string API_ENDPOINT = "https://en.wikipedia.org/w/api.php";

    /// <summary>
    /// Removes HTML formatting and cleans up the input string.
    /// </summary>
    /// <param name="input">The string to be cleaned.</param>
    /// <returns>A cleaned string without HTML tags and extra whitespace.</returns>
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

    /// <summary>
    /// Fetches the content of a Wikipedia article based on the given topic.
    /// </summary>
    /// <param name="topic">The topic to search for on Wikipedia.</param>
    /// <returns>A WikipediaArticle object containing the article information.</returns>
    public static async Task<WikipediaPage> GetWikipediaPage(string topic)
    {
        // I want to differiate between a valid page and Disambiguation pages (as I just found out they are called)

        var query = HttpUtility.UrlEncode(topic);

        // query properties full extract, info (url and page info), up to 500 links
        // links are in alphabetically ordered. Query lots of them and we can randomly select.
        var url = $"{API_ENDPOINT}?action=query&format=json&prop=extracts|info|links&redirects=1&inprop=url|displaytitle&pllimit=100&titles={query}";

        try
        {
            var response = await client.GetStringAsync(url);
            var jsonDoc = JsonDocument.Parse(response);
            var pages = jsonDoc.RootElement.GetProperty("query").GetProperty("pages");

            // get the first valid page of our query
            foreach (var page in pages.EnumerateObject())
            {
                var wikiPage = new WikipediaPage
                {
                    Id = page.Value.GetProperty("pageid").GetInt32(),
                    Title = page.Value.GetProperty("title").GetString(),
                    Extract = RemoveFormatting(page.Value.GetProperty("extract").GetString()),
                    LastModified = DateTime.Parse(page.Value.GetProperty("touched").GetString()),
                    Url = page.Value.GetProperty("fullurl").GetString(),
                    Length = page.Value.GetProperty("length").GetInt32()
                };

                if (page.Value.TryGetProperty("links", out var links))
                {
                    foreach (var link in links.EnumerateArray())
                    {
                        // could do every other link to reduce size
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