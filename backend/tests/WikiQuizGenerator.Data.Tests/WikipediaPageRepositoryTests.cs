using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Xunit;

using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Models;
using WikiQuizGenerator.Data;
using WikiQuizGenerator.Data.Repositories;

namespace WikiQuizGenerator.Tests;

public class WikipediaPageRepositoryTests
{
    private readonly WikiQuizDbContext _context;
    private readonly IWikipediaPageRepository _repository;
    public WikipediaPageRepositoryTests()
    {
        _context = TestContext.CreateInMemoryDatabase();
        _repository = new WikipediaPageRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsWikipediaPage_WhenPageExists()
    {
        // Arrange
        var expectedPage = new WikipediaPage()
        {
            Id = 1,
            Langauge = "en",
            Title = "Test Wikipedia Page",
            Extract = "This is a test Wikipedia page extract.",
            LastModified = DateTime.UtcNow,
            Url = "https://en.wikipedia.org/wiki/Test",
            Length = 100,
            Links = new List<string> { "Link1", "Link2" },
            Categories = new List<string> { "Category1", "Category2" }
        };

        _context.WikipediaPages.Add(expectedPage);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedPage.Id, result.Id);
        Assert.Equal(expectedPage.Title, result.Title);
        Assert.Equal(expectedPage.Langauge, result.Langauge);
        Assert.Equal(expectedPage.Extract, result.Extract);
        Assert.Equal(expectedPage.Url, result.Url);
        Assert.Equal(expectedPage.Length, result.Length);
        Assert.Equal(expectedPage.Links, result.Links);
        Assert.Equal(expectedPage.Categories, result.Categories);
    }
}