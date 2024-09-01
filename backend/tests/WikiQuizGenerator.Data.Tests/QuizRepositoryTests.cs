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

    // [Fact]
    // public async Task AddSubmission_ShouldAddNewSubmissionToDatabase()
    // {
    //     // Arrange
    //     await ResetDatabaseAsync();
    //     var newSubmission = new QuizSubmission
    //     {
    //         QuizId = TestQuiz.Id,
    //         Answers = new List<int> { 2 },
    //         SubmissionTime = DateTime.UtcNow,
    //         Score = 2
    //     };
    //
    //     // Act
    //     var result = await _repository.AddSubmissionAsync(newSubmission);
    //
    //     // Assert
    //     Assert.NotEqual(0, result.Id);
    //     var submissionInDb = await _context.QuizSubmissions.FindAsync(result.Id);
    //     Assert.NotNull(submissionInDb);
    //     Assert.Equal(newSubmission.QuizId, submissionInDb.QuizId);
    //     Assert.Equal(newSubmission.Score, submissionInDb.Score);
    // }

    [Fact]
    public async Task DeleteSubmission_ShouldRemoveSubmissionFromDatabase()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Act
        await _repository.DeleteSubmissionAsync(TestSubmission.Id);

        // Assert
        var deletedSubmission = await _context.QuizSubmissions.FindAsync(TestSubmission.Id);
        Assert.Null(deletedSubmission);
    }

    [Fact]
    public async Task DeleteQuiz_ShouldCascadeDeleteSubmissions()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Act
        await _repository.DeleteAsync(TestQuiz.Id);

        // Assert
        var deletedQuiz = await _context.Quizzes.FindAsync(TestQuiz.Id);
        Assert.Null(deletedQuiz);

        var deletedSubmission = await _context.QuizSubmissions.FindAsync(TestSubmission.Id);
        Assert.Null(deletedSubmission);
    }

    // [Fact]
    // public async Task GetSubmissionById_ShouldReturnCorrectSubmission()
    // {
    //     // Arrange
    //     await ResetDatabaseAsync();
    //
    //     // Act
    //     var result = await _repository.GetSubmissionByIdAsync(TestQuizSubmission.Id);
    //
    //     // Assert
    //     Assert.NotNull(result);
    //     Assert.Equal(TestQuizSubmission.Id, result.Id);
    //     Assert.Equal(TestQuizSubmission.QuizId, result.QuizId);
    //     Assert.Equal(TestQuizSubmission.Score, result.Score);
    // }

    [Fact]
    public async Task GetSubmissionById_ShouldReturnNullForNonexistentId()
    {
        // Arrange
        await ResetDatabaseAsync();
        int nonexistentId = 9999;

        // Act
        var result = await _repository.GetSubmissionByIdAsync(nonexistentId);

        // Assert
        Assert.Null(result);
    }

    // [Fact]
    // public async Task GetAllSubmissions_ShouldReturnAllSubmissions()
    // {
    //     // Arrange
    //     await ResetDatabaseAsync();
    //
    //     // Add another submission
    //     var newSubmission = new QuizSubmission
    //     {
    //         QuizId = TestQuiz.Id,
    //         Answers = new List<int> { 2, 3, 1, 4 },
    //         SubmissionTime = DateTime.UtcNow,
    //         Score = 2
    //     };
    //     await _repository.AddSubmissionAsync(newSubmission);

        // Act
    //     var results = await _repository.GetAllSubmissionsAsync();
    //
    //     // Assert
    //     Assert.Equal(2, results.Count());
    //     Assert.Contains(results, s => s.Id == TestQuizSubmission.Id);
    //     Assert.Contains(results, s => s.Id == newSubmission.Id);
    // }

    [Fact]
    public async Task GetAllSubmissions_ShouldReturnEmptyListWhenNoSubmissions()
    {
        // Arrange
        await ResetDatabaseAsync();
        _context.QuizSubmissions.RemoveRange(_context.QuizSubmissions);
        await _context.SaveChangesAsync();

        // Act
        var results = await _repository.GetAllSubmissionsAsync();

        // Assert
        Assert.Empty(results);
    }
}