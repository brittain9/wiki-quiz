using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Xunit;

using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Models;
using WikiQuizGenerator.Data;
using WikiQuizGenerator.Data.Repositories;

namespace WikiQuizGenerator.Tests;

public class WikipediaPageRepositoryTests : TestBase
{
    private readonly IWikipediaPageRepository _repository;
    public WikipediaPageRepositoryTests()
    {
        _repository = new WikipediaPageRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsWikipediaPage_WhenPageExists()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(TestWikipediaPage.Id, result.Id);
        Assert.Equal(TestWikipediaPage.Title, result.Title);
        // Assert.Equal(TestWikipediaPage.Langauge, result.Langauge);
        Assert.Equal(TestWikipediaPage.Extract, result.Extract);
        Assert.Equal(TestWikipediaPage.Url, result.Url);
        Assert.Equal(TestWikipediaPage.Length, result.Length);
        Assert.Equal(TestWikipediaPage.Links, result.Links);
        Assert.Equal(TestWikipediaPage.Categories, result.Categories);
    }
}