using System;
using System.Threading.Tasks;
using Xunit;

using WikiQuizGenerator.Core.Models;

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
        WikipediaArticle article = await WikipediaContent.GetWikipediaArticle(topic);
        string toStringResult = article.ToString();

        // Assert
        Assert.NotNull(article);
        Assert.NotNull(toStringResult);

        // Check article properties
        Assert.Equal(topic, article.Title);
        Assert.True(article.Content.Length > MinimumContentLength, $"Content length ({article.Content.Length}) is not adequate for a Wikipedia article");
        Assert.Contains(expectedWord, article.Content, StringComparison.OrdinalIgnoreCase);
        Assert.NotEmpty(article.Url);
        Assert.NotEqual(default(DateTime), article.LastModified);
        Assert.NotEmpty(article.Categories);
        // Don't check related links for now

        // Check ToString output
        Assert.Contains(article.Title, toStringResult);
        Assert.Contains(article.Url, toStringResult);
        Assert.Contains(article.LastModified.ToString("yyyy-MM-dd"), toStringResult);
        Assert.Contains("Content:", toStringResult);
        Assert.Contains("Categories:", toStringResult);

        // Optional: Print the ToString result for debugging
        Console.WriteLine($"ToString output for '{topic}':");
        Console.WriteLine(toStringResult);
    }

    [Fact]
    public async Task GetWikipediaArticle_InvalidTopic_ReturnsNull()
    {
        // Arrange
        string invalidTopic = "ThisIsAnInvalidWikipediaTopicThatShouldNotExist12345";

        // Act
        WikipediaArticle article = await WikipediaContent.GetWikipediaArticle(invalidTopic);

        // Assert
        Assert.Null(article);
    }

    [Fact]
    public async Task GetWikipediaArticle_ReturnsValidArticle()
    {
        // Arrange
        string topic = "George Washington";

        // Act
        var article = await WikipediaContent.GetWikipediaArticle(topic);

        // Assert
        Assert.NotNull(article);
        Assert.Equal(topic, article.Title);
        Assert.NotEmpty(article.Content);
        Assert.NotEmpty(article.Url);
        Assert.NotEqual(default(DateTime), article.LastModified);
        Assert.NotEmpty(article.Categories);
    }

    [Fact]
    public void RemoveFormatting_RemovesHTMLTags()
    {
        // Arrange
        string input = "<p>This is <b>bold</b> and <i>italic</i> text.</p>";
        string expected = "This is bold and italic text.";

        // Act
        string result = WikipediaContent.RemoveFormatting(input);

        // Assert
        Assert.Equal(expected, result);
    }
}