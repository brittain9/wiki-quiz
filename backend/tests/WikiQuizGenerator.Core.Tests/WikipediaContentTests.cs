using System;
using System.Threading.Tasks;
using Xunit;

using WikiQuizGenerator.Core.Models;
using System.Diagnostics;
using WikiQuizGenerator.Core;

public class WikipediaContentTests
{
    [Theory]
    [InlineData("George Washington", "Washington")]
    [InlineData("C++", "C plus plus")]
    [InlineData("Mars ocean theory", "Mars")]
    public async Task GetWikipediaArticle_ReturnsValidArticleAndToString(string topic, string expectedWord)
    {
        // Arrange
        const int MinimumContentLength = 100; // Adjust this value as needed

        // Act
        WikipediaPage article = await WikipediaContent.GetWikipediaPage(topic);
        string toStringResult = article.ToString();

        // Assert
        Assert.NotNull(article);
        Assert.NotNull(toStringResult);

        // Check article properties
        Assert.Equal(topic, article.Title);
        Assert.Contains(expectedWord, article.Extract, StringComparison.OrdinalIgnoreCase);
        Assert.NotEmpty(article.Url);
        Assert.NotEqual(default(DateTime), article.LastModified);
        Assert.NotEmpty(article.Links);

        // Print the ToString result for debugging
        Debug.WriteLine($"ToString output for '{topic}':\n");
        Debug.WriteLine(toStringResult);
    }

    [Theory]
    [InlineData("invasion of ygoslavia", "Invasion of Yugoslavia")]
    [InlineData("SURGERY IN ANCIENT ROME", "Surgery in ancient Rome")]
    [InlineData("MarS OceAn TheoRy", "Mars ocean theory")]
    public async Task GetWikipediaExactTitle_ReturnsValidTitle(string input, string expectedTitle)
    {
        string title = await WikipediaContent.GetWikipediaExactTitle(input);

        Assert.Equal(title, expectedTitle);
    }

    [Fact]
    public async Task GetWikipediaArticle_InvalidTopic_ReturnsNull()
    {
        string invalidTopic = "ThisIsAnInvalidWikipediaTopicThatShouldNotExist12345";

        WikipediaPage article = await WikipediaContent.GetWikipediaPage(invalidTopic);

        Assert.Null(article);
    }

    // Next two tests are for debugging the JSON returned and GetWikipediaPage function
    [Fact]
    public async Task DifferentLangauges_WikipediaContent()
    {   
        var englishPage = await WikipediaContent.GetWikipediaPage("Computer Science", "en");
        var spanishPage = await WikipediaContent.GetWikipediaPage("Informática", "es");
        var frenchPage = await WikipediaContent.GetWikipediaPage("Informatique", "fr");

        Assert.NotNull(englishPage);
        Assert.NotNull(spanishPage);
        Assert.NotNull(frenchPage);

        // Debug print
        Console.WriteLine(englishPage.Extract.Substring(0, 50));
        Console.WriteLine(spanishPage.Extract.Substring(0, 50));
        Console.WriteLine(frenchPage.Extract.Substring(0, 50));
    }

    [Fact]
    public async Task DisambiguationPage_WikipediaContent()
    {   
        var disambiguationPage = await WikipediaContent.GetWikipediaPage("George Bush");

        Assert.NotNull(disambiguationPage);

        Console.WriteLine(disambiguationPage.Extract.Substring(0, 50));
    }

    [Fact]
    public void RemoveFormatting_RemovesHTMLTags()
    {
        string input = "<this will be removed> this will stay </this will be removed>";
        string expected = "this will stay";

        string result = WikipediaContent.RemoveFormatting(input);

        Assert.Equal(expected, result);
    }
}