using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Utilities;
using WikiQuizGenerator.Core.Services;

public class WikipediaContentProviderTests
{
    private readonly Mock<ILogger<WikipediaContentService>> _mockLogger;
    private readonly WikipediaContentService _contentProvider;

    public WikipediaContentProviderTests()
    {
        // Setup mocks
        _mockLogger = new Mock<ILogger<WikipediaContentService>>();

        // Create the instance to test
        _contentProvider = new WikipediaContentService(_mockLogger.Object);
    }

    [Theory]
    [InlineData("invasion of ygoslavia", "Invasion of Yugoslavia")]
    [InlineData("SURGERY IN ANCIENT ROME", "Surgery in ancient Rome")]
    [InlineData("MarS OceAn TheoRy", "Mars ocean theory")]
    public async Task GetWikipediaExactTitle_ReturnsValidTitle(string input, string expectedTitle)
    {
        string title = await _contentProvider.GetWikipediaExactTitle(input);

        Assert.Equal(expectedTitle, title);
    }

    [Fact]
    public async Task GetWikipediaExactTitle_InvalidTopic_ReturnsEmpty()
    {
        string invalidTopic = "ThisIsAnInvalidWikipediaTopicThatShouldNotExist12345";

        string title = await _contentProvider.GetWikipediaExactTitle(invalidTopic);

        Assert.Empty(title);
    }

    [Fact]
    public void RemoveFormatting_RemovesHTMLTags()
    {
        string input = "<this will be removed> this will stay </this will be removed>";
        string expected = "this will stay";

        string result = WikipediaContentService.RemoveFormatting(input);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("George Washington", "Washington", "en", 1000)]
    [InlineData("Mars ocean theory", "Mars", "en", 1500)]
    public async Task GetWikipediaContentAsync_ReturnsValidContent(string topic, string expectedWord, string language, int extractLength)
    {
        // Arrange
        Languages lang = LanguagesExtensions.GetLanguageFromCode(language);

        // Act
        var result = await _contentProvider.GetWikipediaContentAsync(topic, lang, extractLength, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.WikipediaReference);
        Assert.NotEmpty(result.ProcessedContent);
        Assert.Contains(expectedWord, result.ProcessedContent, StringComparison.OrdinalIgnoreCase);
        Assert.NotEmpty(result.WikipediaReference.Title);
        Assert.NotEmpty(result.WikipediaReference.Url);
        Assert.Equal(lang.GetWikipediaLanguageCode(), result.WikipediaReference.Language);
    }
}