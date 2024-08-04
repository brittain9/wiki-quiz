using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WikiQuizGenerator.Core;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Models;
using WikiQuizGenerator.Data;
using WikiQuizGenerator.Data.Repositories;
using Xunit;

namespace WikiQuizGenerator.Tests;

public class WikipediaPageRepositoryTests : TestBase
{
    private readonly IWikipediaPageRepository _repository;

    public WikipediaPageRepositoryTests()
    {
        _repository = new WikipediaPageRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectWikipediaPage()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Act
        var result = await _repository.GetByIdAsync(TestWikipediaPage.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(TestWikipediaPage.Id, result.Id);
        Assert.Equal(TestWikipediaPage.Title, result.Title);
        Assert.Equal(TestWikipediaPage.Links.Count, result.Links.Count);
        Assert.Equal(TestWikipediaPage.Categories.Count, result.Categories.Count);
    }

    [Fact]
    public async Task GetByWikipediaIdAsync_ShouldReturnCorrectWikipediaPage()
    {
        // Arrange
        await ResetDatabaseAsync();
        Languages language = LanguagesExtensions.GetLanguageFromCode(TestWikipediaPage.Language);
       
        // Act
        var result = await _repository.GetByWikipediaIdAsync(TestWikipediaPage.WikipediaId, language);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(TestWikipediaPage.Id, result.Id);
        Assert.Equal(TestWikipediaPage.WikipediaId, result.WikipediaId);
        Assert.Equal(TestWikipediaPage.Title, result.Title);
        Assert.Equal(TestWikipediaPage.Links.Count, result.Links.Count);
        Assert.Equal(TestWikipediaPage.Categories.Count, result.Categories.Count);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllWikipediaPages()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Single(result);
        var page = result.First();
        Assert.Equal(TestWikipediaPage.Id, page.Id);
        Assert.Equal(TestWikipediaPage.Title, page.Title);
    }

    [Fact]
    public async Task AddAsync_ShouldAddWikipediaPageToDatabase()
    {
        // Arrange
        await ResetDatabaseAsync();
        var newPage = new WikipediaPage
        {
            Language = "fr",
            Title = "Nouvelle Page",
            Extract = "Ceci est un nouvel extrait.",
            LastModified = DateTime.UtcNow,
            Url = "https://fr.wikipedia.org/wiki/Nouvelle_Page",
            Length = 50,
            Links = new List<string> { "Lien1", "Lien2" },
            Categories = new List<WikipediaCategory> { new WikipediaCategory { Name = "Nouvelle Catégorie" } }
        };

        // Act
        var result = await _repository.AddAsync(newPage);

        // Assert
        Assert.NotEqual(0, result.Id);
        var pageInDb = await _context.WikipediaPages.FindAsync(result.Id);
        Assert.NotNull(pageInDb);
        Assert.Equal(newPage.Title, pageInDb.Title);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveWikipediaPageWhenNoAssociatedAIResponses()
    {
        // Arrange
        await ResetDatabaseAsync();

        // First, delete the associated Quiz
        var quizToDelete = await _context.Quizzes
            .Include(q => q.AIResponses)
            .FirstAsync(q => q.Id == TestQuiz.Id);
        _context.Quizzes.Remove(quizToDelete);
        await _context.SaveChangesAsync();

        // Now, the AIResponse should be automatically deleted due to cascade delete
        // Refresh the Wikipedia page to ensure it's not tracking any removed entities
        var testWikipediaPage = await _context.WikipediaPages
            .Include(wp => wp.AIResponses)
            .FirstAsync(wp => wp.Id == TestWikipediaPage.Id);

        // Verify that the AIResponse has been removed
        Assert.Empty(testWikipediaPage.AIResponses);

        // Act
        var result = await _repository.DeleteAsync(testWikipediaPage.Id);

        // Assert
        Assert.True(result);
        var deletedPage = await _context.WikipediaPages.FindAsync(testWikipediaPage.Id);
        Assert.Null(deletedPage);
    }


    [Fact]
    public async Task DeleteAsync_ShouldNotRemoveWikipediaPageWhenAssociatedAIResponsesExist()
    {
        // Arrange
        await ResetDatabaseAsync();

        // TestWikipediaPage already has an associated TestAIResponse from TestBase

        // Act
        var result = await _repository.DeleteAsync(TestWikipediaPage.Id);

        // Assert
        Assert.False(result);
        var page = await _context.WikipediaPages.FindAsync(TestWikipediaPage.Id);
        Assert.NotNull(page);
        Assert.Equal(TestWikipediaPage.Id, page.Id);
    }

    [Fact]
    public async Task GetByTitleAsync_ShouldReturnCorrectWikipediaPage()
    {
        // Arrange
        await ResetDatabaseAsync();
        var testLanguage = Languages.English; // Assume English for this test, adjust as needed

        // Act
        var result = await _repository.GetByTitleAsync(TestWikipediaPage.Title, testLanguage);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(TestWikipediaPage.Id, result.Id);
        Assert.Equal(TestWikipediaPage.Title, result.Title);
        Assert.Equal(testLanguage.GetWikipediaLanguageCode(), result.Language);
    }

    [Fact]
    public async Task ExistsByTitleAsync_ShouldReturnTrueForExistingTitle()
    {
        // Arrange
        await ResetDatabaseAsync();
        var testLanguage = Languages.English; // Assume English for this test, adjust as needed

        // Act
        var result = await _repository.ExistsByTitleAsync(TestWikipediaPage.Title, testLanguage);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExistsByTitleAsync_ShouldReturnFalseForNonExistingTitle()
    {
        // Arrange
        await ResetDatabaseAsync();
        var testLanguage = Languages.English; // Assume English for this test, adjust as needed

        // Act
        var result = await _repository.ExistsByTitleAsync("Non-existing Title", testLanguage);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ExistsByTitleAsync_ShouldReturnFalseForExistingTitleInDifferentLanguage()
    {
        // Arrange
        await ResetDatabaseAsync();
        var testLanguage = Languages.Spanish; // Assume the page exists in English but not in Spanish

        // Act
        var result = await _repository.ExistsByTitleAsync(TestWikipediaPage.Title, testLanguage);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetByLanguageAsync_ShouldReturnCorrectWikipediaPages()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Act
        var result = await _repository.GetByLanguageAsync(TestWikipediaPage.Language);

        // Assert
        Assert.Single(result);
        var page = result.First();
        Assert.Equal(TestWikipediaPage.Id, page.Id);
        Assert.Equal(TestWikipediaPage.Language, page.Language);
    }

    [Fact]
    public async Task GetByCategoryAsync_ShouldReturnCorrectWikipediaPages()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Act
        var result = await _repository.GetByCategoryAsync(TestWikipediaCategory.Name);

        // Assert
        Assert.Single(result);
        var page = result.First();
        Assert.Equal(TestWikipediaPage.Id, page.Id);
        Assert.Contains(page.Categories, c => c.Name == TestWikipediaCategory.Name);
    }

    [Fact]
    public async Task DeleteEmptyCategoriesAsync_ShouldRemoveEmptyCategories()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Create an empty category
        var emptyCategory = new WikipediaCategory { Name = "Empty Category" };
        _context.WikipediaCategories.Add(emptyCategory);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteEmptyCategoriesAsync();

        // Assert
        var deletedCategory = await _context.WikipediaCategories.FindAsync(emptyCategory.Id);
        Assert.Null(deletedCategory);

        // The TestWikipediaCategory should still exist as it has an associated page
        var existingCategory = await _context.WikipediaCategories.FindAsync(TestWikipediaCategory.Id);
        Assert.NotNull(existingCategory);
    }
}
