using System;
using System.Threading.Tasks;
using Xunit;
using Moq;

using WikiQuizGenerator.Core.Models;
using System.Diagnostics;
using WikiQuizGenerator.Core;
using Microsoft.Extensions.Logging;
using WikiQuizGenerator.Core.Interfaces;

public class WikipediaContentProviderTests
{
    private readonly Mock<IWikipediaPageRepository> _mockPageRepository;
    private readonly Mock<ILogger<WikipediaContentProvider>> _mockLogger;
    private readonly WikipediaContentProvider _contentProvider;

    public WikipediaContentProviderTests()
    {
        // Setup mocks
        _mockPageRepository = new Mock<IWikipediaPageRepository>();
        _mockLogger = new Mock<ILogger<WikipediaContentProvider>>();

        // Create the instance to test
        _contentProvider = new WikipediaContentProvider(_mockPageRepository.Object, _mockLogger.Object);
    }

    [Theory]
    [InlineData("George Washington", "Washington", "en")]
    [InlineData("C++", "C plus plus", "en")]
    [InlineData("Mars ocean theory", "Mars", "en")]
    [InlineData("Informática", "Informática", "es")]
    [InlineData("Informatique", "Informatique", "fr")]
    public async Task GetWikipediaArticle_ReturnsValidArticle(string topic, string expectedWord, string language)
    {
        // Arrange
        Languages lang = LanguagesExtensions.GetLanguageFromCode(language);

        _mockPageRepository.Setup(repo => repo.ExistsByTitleAsync(topic, lang)).ReturnsAsync(false);
        _mockPageRepository.Setup(repo => repo.AddAsync(It.IsAny<WikipediaPage>())).ReturnsAsync((WikipediaPage page) => page); // This will return the same page that was passed in
        
        // Act
        WikipediaPage page = await _contentProvider.GetWikipediaPage(topic, lang);

        // Assert
        Assert.NotNull(page);

        Assert.Equal(lang.GetWikipediaLanguageCode(), page.Language);
        Assert.Contains(expectedWord, page.Extract, StringComparison.OrdinalIgnoreCase);
        Assert.NotEmpty(page.Url);
        Assert.NotEmpty(page.Links);
        Assert.NotEmpty(page.Categories);
    }

    [Theory]
    [InlineData("invasion of ygoslavia", "Invasion of Yugoslavia")]
    [InlineData("SURGERY IN ANCIENT ROME", "Surgery in ancient Rome")]
    [InlineData("MarS OceAn TheoRy", "Mars ocean theory")]
    public async Task GetWikipediaExactTitle_ReturnsValidTitle(string input, string expectedTitle)
    {
        string title = await _contentProvider.GetWikipediaExactTitle(input);

        Assert.Equal(title, expectedTitle);
    }

    [Fact]
    public async Task GetWikipediaExactTitle_InvalidTopic_ReturnsNull()
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

        string result = WikipediaContentProvider.RemoveFormatting(input);

        Assert.Equal(expected, result);
    }
}