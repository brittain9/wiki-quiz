using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.DomainObjects;
using WikiQuizGenerator.Data.Cosmos.Entities;

namespace WikiQuizGenerator.Data.Cosmos.Repositories;

public class CosmosQuizRepository : IQuizRepository
{
    private readonly CosmosDbContext _context;

    public CosmosQuizRepository(CosmosDbContext context)
    {
        _context = context;
    }

    public async Task<Quiz> AddAsync(Quiz coreQuiz)
    {
        // Generate a simple integer ID using time-based approach (placeholder)
        coreQuiz.Id = (int)(DateTime.UtcNow.Ticks % int.MaxValue);
        var doc = MapToDocument(coreQuiz);
        var response = await _context.QuizContainer.CreateItemAsync(doc, new PartitionKey(doc.PartitionKey));
        return MapFromDocument(response.Resource);
    }

    public async Task<Quiz?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            var query = _context.QuizContainer.GetItemLinqQueryable<QuizDocument>()
                .Where(q => q.Id == id.ToString())
                .ToFeedIterator();

            var results = await query.ReadNextAsync(cancellationToken);
            var quiz = results.FirstOrDefault();
            return quiz != null ? MapFromDocument(quiz) : null;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IEnumerable<Quiz>> GetAllAsync()
    {
        var iterator = _context.QuizContainer
            .GetItemLinqQueryable<QuizDocument>()
            .ToFeedIterator();

        var quizzes = new List<Quiz>();
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            var mappedQuizzes = response.Select(MapFromDocument);
            quizzes.AddRange(mappedQuizzes);
        }

        return quizzes;
    }

    public async Task<IEnumerable<Submission>> GetRecentQuizSubmissionsAsync(int count = 10)
    {
        var iterator = _context.QuizContainer
            .GetItemLinqQueryable<QuizDocument>()
            .Where(q => q.Submission != null)
            .OrderByDescending(q => q.Submission!.SubmissionTime)
            .Take(count)
            .ToFeedIterator();

        var submissions = new List<Submission>();
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            foreach (var quiz in response)
            {
                if (quiz.Submission != null)
                {
                    submissions.Add(MapSubmissionFromDocument(quiz.Submission));
                }
            }
        }

        return submissions;
    }

    public async Task DeleteAsync(int id)
    {
        // Find the quiz first to get the partition key
        var quiz = await GetByIdAsync(id, CancellationToken.None);
        if (quiz != null)
        {
            await _context.QuizContainer.DeleteItemAsync<QuizDocument>(
                id.ToString(),
                new PartitionKey(quiz.CreatedBy.ToString()));
        }
    }

    public async Task<Submission?> AddSubmissionAsync(int quizId, Submission submission, CancellationToken cancellationToken)
    {
        // Find the quiz and add the submission to it
        var quiz = await GetByIdAsync(quizId, cancellationToken);
        if (quiz == null) return null;

        var doc = MapToDocument(quiz);
        doc.Submission = MapSubmissionToDocument(submission);

        var response = await _context.QuizContainer.ReplaceItemAsync(
            doc,
            doc.Id,
            new PartitionKey(doc.PartitionKey),
            cancellationToken: cancellationToken);

        return response.Resource.Submission != null ? MapSubmissionFromDocument(response.Resource.Submission) : null;
    }

    // Document shape persisted in Cosmos
    // moved to Entities/

    private static QuizDocument MapToDocument(Quiz core)
    {
        return new QuizDocument
        {
            Id = core.Id.ToString(),
            PartitionKey = core.CreatedBy.ToString(),
            Title = core.Title,
            CreatedAt = core.CreatedAt,
            CreatedBy = core.CreatedBy,
            WikipediaReference = core.WikipediaReference,
            Questions = core.Questions.Select(q => new QuestionDocument
            {
                Text = q.Text,
                Options = q.Options.ToList(),
                CorrectAnswerIndex = q.CorrectAnswerIndex
            }).ToList(),
            Submission = core.Submission != null ? MapSubmissionToDocument(core.Submission) : null,
            InputTokenCount = core.InputTokenCount,
            OutputTokenCount = core.OutputTokenCount,
            ResponseTimeMs = core.ResponseTimeMs,
            ModelId = core.ModelId,
            EstimatedCostUsd = core.EstimatedCostUsd
        };
    }

    private static Quiz MapFromDocument(QuizDocument doc)
    {
        return new Quiz
        {
            Id = int.Parse(doc.Id),
            Title = doc.Title,
            CreatedAt = doc.CreatedAt,
            CreatedBy = doc.CreatedBy,
            WikipediaReference = doc.WikipediaReference,
            Questions = doc.Questions.Select(q => new Question
            {
                Text = q.Text,
                Options = q.Options,
                CorrectAnswerIndex = q.CorrectAnswerIndex
            }).ToList(),
            Submission = doc.Submission != null ? MapSubmissionFromDocument(doc.Submission) : null,
            InputTokenCount = doc.InputTokenCount,
            OutputTokenCount = doc.OutputTokenCount,
            ResponseTimeMs = doc.ResponseTimeMs,
            ModelId = doc.ModelId,
            EstimatedCostUsd = doc.EstimatedCostUsd
        };
    }

    private static SubmissionDocument MapSubmissionToDocument(Submission core)
    {
        return new SubmissionDocument
        {
            Id = 1,
            UserId = core.UserId,
            SubmissionTime = core.SubmissionTime,
            Score = core.Score,
            PointsEarned = core.PointsEarned,
            Answers = core.Answers.ToList()
        };
    }

    private static Submission MapSubmissionFromDocument(SubmissionDocument doc)
    {
        return new Submission
        {
            UserId = doc.UserId,
            SubmissionTime = doc.SubmissionTime,
            Score = doc.Score,
            PointsEarned = doc.PointsEarned,
            Answers = doc.Answers.ToList()
        };
    }

    // Implement remaining interface methods with NotImplementedException for now
    public Task DeleteSubmissionAsync(int submissionId) => throw new NotImplementedException();
    public Task<Submission> GetSubmissionByIdAsync(int submissionId) => throw new NotImplementedException();
    public Task<IEnumerable<Submission>> GetAllSubmissionsAsync() => throw new NotImplementedException();
    public Task<IEnumerable<Submission>> GetSubmissionsByUserIdAsync(Guid userId) => throw new NotImplementedException();
    public Task<(IEnumerable<Submission> submissions, int totalCount)> GetSubmissionsByUserIdPaginatedAsync(Guid userId, int page, int pageSize)
    {
        // TODO: Implement proper Cosmos DB submission querying
        // For now, return empty result to prevent crashes
        return Task.FromResult((Enumerable.Empty<Submission>(), 0));
    }
    public Task<Submission?> GetUserSubmissionByIdAsync(int submissionId, Guid userId) => throw new NotImplementedException();
    public Task<Submission?> GetUserSubmissionByQuizIdAsync(int quizId, Guid userId, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<Question?> GetQuestionByIdAsync(int questionId, CancellationToken cancellationToken) => throw new NotImplementedException();
}