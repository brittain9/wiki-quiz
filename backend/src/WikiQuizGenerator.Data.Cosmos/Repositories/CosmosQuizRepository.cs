using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.DomainObjects;
using CosmosQuiz = WikiQuizGenerator.Data.Cosmos.Models.Quiz;
using CosmosSubmission = WikiQuizGenerator.Data.Cosmos.Models.Submission;
using CosmosQuestion = WikiQuizGenerator.Data.Cosmos.Models.Question;

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
        var cosmosQuiz = MapToCosmos(coreQuiz);
        cosmosQuiz.PartitionKey = coreQuiz.CreatedBy.ToString();
        
        var response = await _context.QuizContainer.CreateItemAsync(cosmosQuiz, new PartitionKey(cosmosQuiz.PartitionKey));
        
        return MapFromCosmos(response.Resource);
    }

    public async Task<Quiz?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            // Since we don't know the CreatedBy for this overload, we need to query
            var query = _context.QuizContainer.GetItemLinqQueryable<CosmosQuiz>()
                .Where(q => q.Id == id.ToString())
                .ToFeedIterator();

            var results = await query.ReadNextAsync(cancellationToken);
            var quiz = results.FirstOrDefault();
            
            return quiz != null ? MapFromCosmos(quiz) : null;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IEnumerable<Quiz>> GetAllAsync()
    {
        var iterator = _context.QuizContainer
            .GetItemLinqQueryable<CosmosQuiz>()
            .ToFeedIterator();

        var quizzes = new List<Quiz>();
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            var mappedQuizzes = response.Select(MapFromCosmos);
            quizzes.AddRange(mappedQuizzes);
        }

        return quizzes;
    }

    public async Task<IEnumerable<Submission>> GetRecentQuizSubmissionsAsync(int count = 10)
    {
        var iterator = _context.QuizContainer
            .GetItemLinqQueryable<CosmosQuiz>()
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
                    submissions.Add(MapSubmissionFromCosmos(quiz.Submission));
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
            await _context.QuizContainer.DeleteItemAsync<CosmosQuiz>(
                id.ToString(), 
                new PartitionKey(quiz.CreatedBy.ToString()));
        }
    }

    public async Task<Submission?> AddSubmissionAsync(Submission submission, CancellationToken cancellationToken)
    {
        // Find the quiz and add the submission to it
        var quiz = await GetByIdAsync(submission.QuizId, cancellationToken);
        if (quiz == null) return null;

        var cosmosQuiz = MapToCosmos(quiz);
        cosmosQuiz.Submission = MapSubmissionToCosmos(submission);

        var response = await _context.QuizContainer.ReplaceItemAsync(
            cosmosQuiz, 
            cosmosQuiz.Id, 
            new PartitionKey(cosmosQuiz.PartitionKey),
            cancellationToken: cancellationToken);

        return response.Resource.Submission != null ? MapSubmissionFromCosmos(response.Resource.Submission) : null;
    }

    // Simple mapping methods - no more Wikipedia page repository dependency
    private CosmosQuiz MapToCosmos(Quiz core)
    {
        // Get Wikipedia reference from the first AIResponse
        var firstAIResponse = core.AIResponses.FirstOrDefault();
        var wikipediaSource = new Models.WikipediaReference();
        
        if (firstAIResponse?.WikipediaReference != null)
        {
            wikipediaSource = new Models.WikipediaReference
            {
                PageId = firstAIResponse.WikipediaReference.PageId,
                Title = firstAIResponse.WikipediaReference.Title,
                Url = firstAIResponse.WikipediaReference.Url,
                Language = firstAIResponse.WikipediaReference.Language
            };
        }

        return new CosmosQuiz
        {
            Id = core.Id.ToString(),
            Title = core.Title,
            CreatedAt = core.CreatedAt,
            CreatedBy = core.CreatedBy,
            WikipediaSource = wikipediaSource,
            AIResponses = core.AIResponses.Select(MapAIResponseToCosmos).ToList(),
            Submission = core.QuizSubmissions.FirstOrDefault() != null ? MapSubmissionToCosmos(core.QuizSubmissions.First()) : null
        };
    }

    private Models.AIResponse MapAIResponseToCosmos(AIResponse core)
    {
        return new Models.AIResponse
        {
            Id = core.Id,
            ResponseTime = core.ResponseTime,
            InputTokenCount = core.InputTokenCount,
            OutputTokenCount = core.OutputTokenCount,
            ModelConfigId = core.ModelConfigId,
            Questions = core.Questions.Select(MapQuestionToCosmos).ToList()
        };
    }

    private Models.Question MapQuestionToCosmos(Question core)
    {
        var options = new List<string> { core.Option1, core.Option2 };
        if (!string.IsNullOrEmpty(core.Option3)) options.Add(core.Option3);
        if (!string.IsNullOrEmpty(core.Option4)) options.Add(core.Option4);
        if (!string.IsNullOrEmpty(core.Option5)) options.Add(core.Option5);

        return new Models.Question
        {
            Id = core.Id,
            Text = core.Text,
            Options = options,
            CorrectOptionNumber = core.CorrectOptionNumber,
            PointValue = core.PointValue
        };
    }

    private Models.Submission MapSubmissionToCosmos(Submission core)
    {
        return new Models.Submission
        {
            Id = core.Id,
            UserId = core.UserId,
            SubmissionTime = core.SubmissionTime,
            Score = core.Score,
            PointsEarned = core.PointsEarned,
            Answers = core.Answers.Select(a => new Models.QuestionAnswer
            {
                Id = a.Id,
                QuestionId = a.QuestionId,
                SelectedOptionNumber = a.SelectedOptionNumber
            }).ToList()
        };
    }

    // Reverse mapping methods - creates WikipediaReference from stored reference
    private Quiz MapFromCosmos(CosmosQuiz cosmos)
    {
        // Create a WikipediaReference from stored data
        WikipediaReference? wikipediaReference = null;
        if (cosmos.WikipediaSource.PageId > 0)
        {
            wikipediaReference = new WikipediaReference
            {
                PageId = cosmos.WikipediaSource.PageId,
                Title = cosmos.WikipediaSource.Title,
                Url = cosmos.WikipediaSource.Url,
                Language = cosmos.WikipediaSource.Language
            };
        }

        var coreQuiz = new Quiz
        {
            Id = int.Parse(cosmos.Id),
            Title = cosmos.Title,
            CreatedAt = cosmos.CreatedAt,
            CreatedBy = cosmos.CreatedBy,
            AIResponses = cosmos.AIResponses.Select(ai => MapAIResponseFromCosmos(ai, wikipediaReference)).ToList()
        };

        // Add submission if it exists
        if (cosmos.Submission != null)
        {
            coreQuiz.QuizSubmissions = new List<Submission> { MapSubmissionFromCosmos(cosmos.Submission) };
        }

        return coreQuiz;
    }

    private AIResponse MapAIResponseFromCosmos(Models.AIResponse cosmos, WikipediaReference? wikipediaReference)
    {
        return new AIResponse
        {
            Id = cosmos.Id,
            ResponseTime = cosmos.ResponseTime,
            InputTokenCount = cosmos.InputTokenCount,
            OutputTokenCount = cosmos.OutputTokenCount,
            ModelConfigId = cosmos.ModelConfigId,
            WikipediaReference = wikipediaReference,
            Questions = cosmos.Questions.Select(MapQuestionFromCosmos).ToList()
        };
    }

    private Question MapQuestionFromCosmos(Models.Question cosmos)
    {
        return new Question
        {
            Id = cosmos.Id,
            Text = cosmos.Text,
            Option1 = cosmos.Options.ElementAtOrDefault(0) ?? "",
            Option2 = cosmos.Options.ElementAtOrDefault(1) ?? "",
            Option3 = cosmos.Options.ElementAtOrDefault(2),
            Option4 = cosmos.Options.ElementAtOrDefault(3),
            Option5 = cosmos.Options.ElementAtOrDefault(4),
            CorrectOptionNumber = cosmos.CorrectOptionNumber,
            PointValue = cosmos.PointValue
        };
    }

    private Submission MapSubmissionFromCosmos(Models.Submission cosmos)
    {
        return new Submission
        {
            Id = cosmos.Id,
            UserId = cosmos.UserId,
            SubmissionTime = cosmos.SubmissionTime,
            Score = cosmos.Score,
            PointsEarned = cosmos.PointsEarned,
            Answers = cosmos.Answers.Select(a => new QuestionAnswer
            {
                Id = a.Id,
                QuestionId = a.QuestionId,
                SelectedOptionNumber = a.SelectedOptionNumber
            }).ToList()
        };
    }

    // Implement remaining interface methods with NotImplementedException for now
    public Task DeleteSubmissionAsync(int submissionId) => throw new NotImplementedException();
    public Task<Submission> GetSubmissionByIdAsync(int submissionId) => throw new NotImplementedException();
    public Task<IEnumerable<Submission>> GetAllSubmissionsAsync() => throw new NotImplementedException();
    public Task<IEnumerable<Submission>> GetSubmissionsByUserIdAsync(Guid userId) => throw new NotImplementedException();
    public Task<(IEnumerable<Submission> submissions, int totalCount)> GetSubmissionsByUserIdPaginatedAsync(Guid userId, int page, int pageSize) => throw new NotImplementedException();
    public Task<Submission?> GetUserSubmissionByIdAsync(int submissionId, Guid userId) => throw new NotImplementedException();
    public Task<Submission?> GetUserSubmissionByQuizIdAsync(int quizId, Guid userId, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<Question?> GetQuestionByIdAsync(int questionId, CancellationToken cancellationToken) => throw new NotImplementedException();
}