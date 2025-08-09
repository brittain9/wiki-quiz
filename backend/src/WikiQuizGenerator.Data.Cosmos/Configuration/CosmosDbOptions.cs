namespace WikiQuizGenerator.Data.Cosmos.Configuration;

public class CosmosDbOptions
{
    public const string SectionName = "CosmosDb";
    
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "WikiQuizDb";
    public string QuizContainerName { get; set; } = "Quizzes";
    public string UserContainerName { get; set; } = "Users";
}