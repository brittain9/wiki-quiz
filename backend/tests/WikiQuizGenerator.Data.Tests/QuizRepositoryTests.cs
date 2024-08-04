using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Models;
using WikiQuizGenerator.Data;
using WikiQuizGenerator.Data.Repositories;
using Xunit;

namespace WikiQuizGenerator.Tests;

public class QuizRepositoryTests : TestBase
{
    private readonly IQuizRepository _repository;

    public QuizRepositoryTests()
    {
        _repository = new QuizRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddQuizToDatabase()
    {
        // Arrange
        await ResetDatabaseAsync();
        var newQuiz = new Quiz
        {
            Title = "New Test Quiz",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _repository.AddAsync(newQuiz);

        // Assert
        Assert.NotEqual(0, result.Id);
        var quizInDb = await _context.Quizzes.FindAsync(result.Id);
        Assert.NotNull(quizInDb);
        Assert.Equal(newQuiz.Title, quizInDb.Title);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveQuizFromDatabase()
    {
        // Arrange
        await ResetDatabaseAsync();
        var quizToDelete = TestQuiz;

        // Act
        await _repository.DeleteAsync(quizToDelete.Id);

        // Assert
        var deletedQuiz = await _context.Quizzes.FindAsync(quizToDelete.Id);
        Assert.Null(deletedQuiz);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllQuizzes()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.Single(result);
        var quiz = result.First();
        Assert.Equal(TestQuiz.Id, quiz.Id);
        Assert.Equal(TestQuiz.Title, quiz.Title);
        Assert.Single(quiz.AIResponses);
        Assert.Single(quiz.AIResponses.First().Questions);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectQuiz()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Act
        var result = await _repository.GetByIdAsync(TestQuiz.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(TestQuiz.Id, result.Id);
        Assert.Equal(TestQuiz.Title, result.Title);
        Assert.Single(result.AIResponses);
        Assert.Single(result.AIResponses.First().Questions);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNullForNonexistentId()
    {
        // Arrange
        await ResetDatabaseAsync();
        int nonexistentId = 9999;

        // Act
        var result = await _repository.GetByIdAsync(nonexistentId);

        // Assert
        Assert.Null(result);
    }
}
