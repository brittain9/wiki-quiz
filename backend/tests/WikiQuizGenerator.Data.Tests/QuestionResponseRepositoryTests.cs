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
    public class QuestionResponseRepositoryTests
    {
        private readonly WikiQuizDbContext _context;
        private readonly IQuestionResponseRepository _repository;
        public QuestionResponseRepositoryTests()
        {
            _context = TestContext.CreateInMemoryDatabase();
            _repository = new QuestionResponseRepository(_context);
        }




        [Fact]
        public async Task GetByIdAsync_ReturnsQuestionResponse_WhenQuestionResponseExists()
        {
            // Arrange
            var expectedQuestionResponse = new QuestionResponse()
            {
                Id = 1,
                PromptTokenUsage = 10,
                CompletionTokenUsage = 20,
                AIResponseTime = 1000,
                ModelName = "GPT-4",
                WikipediaPageId = 1
            };

            _context.QuestionResponses.Add(expectedQuestionResponse);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedQuestionResponse.Id, result.Id);
            Assert.Equal(expectedQuestionResponse.PromptTokenUsage, result.PromptTokenUsage);
            Assert.Equal(expectedQuestionResponse.CompletionTokenUsage, result.CompletionTokenUsage);
            Assert.Equal(expectedQuestionResponse.AIResponseTime, result.AIResponseTime);
            Assert.Equal(expectedQuestionResponse.ModelName, result.ModelName);
            Assert.Equal(expectedQuestionResponse.WikipediaPageId, result.WikipediaPageId);
        }
    }
}
