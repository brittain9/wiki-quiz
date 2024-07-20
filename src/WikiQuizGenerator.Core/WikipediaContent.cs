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

        // Remove <p class="mw-empty-elt"></p>
        input = Regex.Replace(input, @"<p\s+class=""mw-empty-elt""\s*>\s*</p>", string.Empty);

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
    public static async Task<WikipediaArticle> GetWikipediaArticle(string topic)
    {
        var query = HttpUtility.UrlEncode(topic);
        var url = $"{API_ENDPOINT}?action=query&format=json&prop=extracts|categories|info&exintro=true&redirects=1&inprop=url|displaytitle&titles={query}";

        try
        {
            var response = await client.GetStringAsync(url);
            var jsonDoc = JsonDocument.Parse(response);
            var pages = jsonDoc.RootElement.GetProperty("query").GetProperty("pages");

            foreach (var page in pages.EnumerateObject())
            {
                var article = new WikipediaArticle
                {
                    Title = page.Value.GetProperty("title").GetString(),
                    Content = RemoveFormatting(page.Value.GetProperty("extract").GetString()),
                    Url = page.Value.GetProperty("fullurl").GetString(),
                    LastModified = DateTime.Parse(page.Value.GetProperty("touched").GetString())
                };

                if (page.Value.TryGetProperty("categories", out var categories))
                {
                    foreach (var category in categories.EnumerateArray())
                    {
                        article.Categories.Add(category.GetProperty("title").GetString());
                    }
                }

                return article;
            }

            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Fetches related links for a given Wikipedia article.
    /// </summary>
    /// <param name="article">The WikipediaArticle to find related links for.</param>
    /// <returns>The updated WikipediaArticle with related links.</returns>
    public static async Task<WikipediaArticle> GetRelatedLinks(WikipediaArticle article)
    {
        var query = HttpUtility.UrlEncode(article.Title);
        var url = $"{API_ENDPOINT}?action=query&titles={query}&prop=links&pllimit=50&format=json";

        try
        {
            var response = await client.GetStringAsync(url);
            var jsonDoc = JsonDocument.Parse(response);
            var pages = jsonDoc.RootElement.GetProperty("query").GetProperty("pages");

            foreach (var page in pages.EnumerateObject())
            {
                if (page.Value.TryGetProperty("links", out var linksProperty))
                {
                    foreach (var link in linksProperty.EnumerateArray())
                    {
                        article.RelatedLinks.Add(link.GetProperty("title").GetString());
                    }
                }
            }

            return article;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while fetching related links: {ex.Message}");
            return article;
        }
    }
}