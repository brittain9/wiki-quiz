using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Models;
using WikiQuizGenerator.Data.Repositories;
using Xunit;

namespace WikiQuizGenerator.Data.Tests
{
    public class QuestionRepositoryTests
    {
        private readonly WikiQuizDbContext _context;
        private readonly IQuestionRepository _repository;
        public QuestionRepositoryTests()
        {
            _context = TestContext.CreateInMemoryDatabase();
            _repository = new QuestionRepository(_context);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsQuestion_WhenQuestionExists()
        {
            // Arrange
            var expectedQuestion = new Question()
            {
                Id = 1,
                Text = "What is this test question?",
                Options = new List<string> { "Option A", "Option B", "Option C", "Option D" },
                CorrectAnswerIndex = 0,
                QuestionResponseId = 1
            };

            _context.Questions.Add(expectedQuestion);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedQuestion.Id, result.Id);
            Assert.Equal(expectedQuestion.Text, result.Text);
            Assert.Equal(expectedQuestion.Options, result.Options);
            Assert.Equal(expectedQuestion.CorrectAnswerIndex, result.CorrectAnswerIndex);
            Assert.Equal(expectedQuestion.QuestionResponseId, result.QuestionResponseId);
        }
    }
}
