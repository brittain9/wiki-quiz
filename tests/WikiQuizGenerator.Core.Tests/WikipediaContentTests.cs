using System;
using System.Threading.Tasks;
using Xunit;

using WikiQuizGenerator.Core.Models;
using System.Diagnostics;

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

    [Fact]
    public async Task GetWikipediaArticle_InvalidTopic_ReturnsNull()
    {
        string invalidTopic = "ThisIsAnInvalidWikipediaTopicThatShouldNotExist12345";

        WikipediaPage article = await WikipediaContent.GetWikipediaPage(invalidTopic);

        Assert.Null(article);
    }

    [Fact]
    public async Task GetWikipediaArticle_ReturnsValidArticle()
    {
        string topic = "George Washington";

        var article = await WikipediaContent.GetWikipediaPage(topic);

        Assert.NotNull(article);
        Assert.Equal(topic, article.Title);
        Assert.NotEmpty(article.Extract);
        Assert.NotEmpty(article.Url);
        Assert.NotEqual(default(DateTime), article.LastModified);
        Assert.NotEmpty(article.Links);
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