using System.Web;
using System.Text.Json;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text;
using Microsoft.Extensions.Logging;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.DomainObjects;
using WikiQuizGenerator.Core.Services;
using WikiQuizGenerator.Core.Utilities;

namespace WikiQuizGenerator.Core.Services;

public class WikipediaContentService : IWikipediaContentService, IDisposable
{
    private readonly HttpClient _client;
    private readonly ILogger<WikipediaContentService> _logger;
    private static readonly Random Random = new();
    public Languages Language { get; set; }
    public string ApiEndpoint => $"https://{Language.GetWikipediaLanguageCode()}.wikipedia.org/w/api.php";

    public string QueryApiEndpoint => $"{ApiEndpoint}?action=query&format=json&prop=extracts|info&redirects=1&inprop=url|displaytitle&titles=";

    public WikipediaContentService(ILogger<WikipediaContentService> logger)
    {
        _logger = logger;
        _client = new HttpClient(); // TODO: change this to be more performant
        Language = Languages.English;
    }

    /// <summary>
    /// Gets Wikipedia content with random sections and returns the Wikipedia reference and processed content
    /// </summary>
    public async Task<WikipediaContentResult> GetWikipediaContentAsync(
        string topic, 
        Languages language, 
        int extractLength,
        CancellationToken cancellationToken)
    {
        _logger.LogTrace("Getting wikipedia content on topic '{Topic}' in '{Language}' with length {ExtractLength}.", 
            topic, language.GetWikipediaLanguageCode(), extractLength);

        if (Language != language) 
            Language = language;

        var sw = Stopwatch.StartNew();

        // Get the exact wikipedia page title using the wikipedia api search
        var exactTitle = await GetWikipediaExactTitle(topic);
        if (string.IsNullOrEmpty(exactTitle))
        {
            throw new ArgumentException($"No Wikipedia page found for the given query.", nameof(topic));
        }

        _logger.LogInformation("Got exact article name '{ExactTitle}' from user entered topic '{Topic}' in {ElapsedMilliseconds} milliseconds.", 
            exactTitle, topic, sw.ElapsedMilliseconds);
        
        sw.Restart();

        var exactQuery = HttpUtility.UrlEncode(exactTitle);
        var exactUrl = QueryApiEndpoint + exactQuery;

        try
        {
            var response = await _client.GetStringAsync(exactUrl, cancellationToken);
            var jsonDoc = JsonDocument.Parse(response);
            var pages = jsonDoc.RootElement.GetProperty("query").GetProperty("pages");

            // Get the first valid page of our query
            foreach (var page in pages.EnumerateObject())
            {
                var wikipediaId = page.Value.GetProperty("pageid").GetInt32();
                var title = page.Value.GetProperty("title").GetString() ?? string.Empty;
                var pageLanguage = page.Value.GetProperty("pagelanguage").GetString() ?? language.GetWikipediaLanguageCode();
                var fullExtract = RemoveFormatting(page.Value.GetProperty("extract").GetString());
                var url = page.Value.GetProperty("fullurl").GetString() ?? string.Empty;

                // Create Wikipedia reference
                var wikipediaReference = new WikipediaReference
                {
                    PageId = wikipediaId,
                    Title = title,
                    Url = url,
                    Language = pageLanguage
                };

                // Get random content sections from the full extract
                var processedContent = GetRandomContentSections(fullExtract, extractLength);

                sw.Stop();
                _logger.LogInformation("Fetched and processed Wikipedia content for '{Title}' with language '{Language}' in {ElapsedMilliseconds} milliseconds.", 
                    title, language.GetWikipediaLanguageCode(), sw.ElapsedMilliseconds);
                
                return new WikipediaContentResult
                {
                    WikipediaReference = wikipediaReference,
                    ProcessedContent = processedContent
                };
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

    /// <summary>
    /// Extracts random content sections from Wikipedia extract
    /// </summary>
    private static string GetRandomContentSections(string extract, int requestedContentLength, int minSeparation = 0, string sectionSeparator = "\n\n")
    {
        const int MinContentLength = 500;
        const int MaxContentLength = 50000;
        const int LengthForLargerSections = 3000;
        const int LargerSectionSize = 1000;
        const int SmallerSectionSize = 500;

        if (string.IsNullOrEmpty(extract))
            return string.Empty;

        int contentLength = Math.Clamp(requestedContentLength, MinContentLength, MaxContentLength);
        if (contentLength >= extract.Length)
            return extract;

        int sectionSize = contentLength >= LengthForLargerSections ? LargerSectionSize : SmallerSectionSize;
        var result = new StringBuilder(contentLength);
        var usedRanges = new List<(int Start, int End)>();  // Sorted list of used ranges

        int targetLength = Math.Min(contentLength, extract.Length);
        while (result.Length < targetLength)
        {
            int remainingSpace = targetLength - result.Length;
            int currentSectionSize = Math.Min(sectionSize, remainingSpace);
            int maxStart = extract.Length - currentSectionSize;

            // Find a non-overlapping random position
            int? startPosition = FindAvailablePosition(extract.Length, usedRanges, currentSectionSize, minSeparation, maxStart);
            if (startPosition == null)
            {
                // Fallback: Fill remaining with largest available non-used chunks
                FillRemaining(extract, targetLength, result, usedRanges, sectionSeparator);
                break;
            }

            // Add the section with optional separator
            if (result.Length > 0)
                result.Append(sectionSeparator);
            result.Append(extract, startPosition.Value, currentSectionSize);

            // Insert the new range into the sorted list
            InsertSorted(usedRanges, startPosition.Value, startPosition.Value + currentSectionSize);
        }

        return result.ToString();
    }

    private static int? FindAvailablePosition(int extractLength, List<(int Start, int End)> usedRanges, int sectionSize, int minSeparation, int maxStart)
    {
        const int MaxAttempts = 100;
        for (int attempt = 0; attempt < MaxAttempts; attempt++)
        {
            int start = Random.Next(maxStart + 1);
            int end = start + sectionSize;

            if (IsAvailable(usedRanges, start, end, minSeparation))
                return start;
        }
        return null;  // No position found after attempts
    }

    private static bool IsAvailable(List<(int Start, int End)> usedRanges, int proposedStart, int proposedEnd, int minSeparation)
    {
        // Binary search to find potential overlapping ranges
        int index = BinarySearchForOverlap(usedRanges, proposedStart);
        for (int i = Math.Max(0, index - 1); i < usedRanges.Count && usedRanges[i].Start < proposedEnd; i++)
        {
            var (existingStart, existingEnd) = usedRanges[i];
            if (proposedEnd > existingStart - minSeparation && proposedStart < existingEnd + minSeparation)
                return false;
        }
        return true;
    }

    private static int BinarySearchForOverlap(List<(int Start, int End)> usedRanges, int value)
    {
        int low = 0, high = usedRanges.Count - 1;
        while (low <= high)
        {
            int mid = (low + high) / 2;
            if (usedRanges[mid].Start < value)
                low = mid + 1;
            else
                high = mid - 1;
        }
        return low;
    }

    private static void InsertSorted(List<(int Start, int End)> usedRanges, int start, int end)
    {
        int index = BinarySearchForOverlap(usedRanges, start);
        usedRanges.Insert(index, (start, end));
    }

    private static void FillRemaining(string extract, int targetLength, StringBuilder result, List<(int Start, int End)> usedRanges, string sectionSeparator)
    {
        // Collect available gaps and sort them by size descending for largest first
        var gaps = GetAvailableGaps(extract.Length, usedRanges);
        gaps.Sort((a, b) => b.Length.CompareTo(a.Length));  // Largest first

        int remaining = targetLength - result.Length;
        foreach (var gap in gaps)
        {
            if (remaining <= 0) break;
            int lengthToTake = Math.Min(remaining, gap.Length);
            if (result.Length > 0)
                result.Append(sectionSeparator);
            result.Append(extract, gap.Start, lengthToTake);
            remaining -= lengthToTake;
            // Note: We don't update usedRanges here as this is fallback
        }
    }

    private static List<(int Start, int Length)> GetAvailableGaps(int extractLength, List<(int Start, int End)> usedRanges)
    {
        var gaps = new List<(int Start, int Length)>();
        int current = 0;
        foreach (var range in usedRanges)
        {
            if (current < range.Start)
                gaps.Add((current, range.Start - current));
            current = Math.Max(current, range.End);
        }
        if (current < extractLength)
            gaps.Add((current, extractLength - current));
        return gaps;
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