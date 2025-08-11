using System.Text.Json.Serialization;

namespace WikiQuizGenerator.Data.Cosmos.Entities;

public sealed class UserDocument
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("partitionKey")] public string PartitionKey { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAtUtc { get; set; }
    public bool IsPremium { get; set; }
    public double TotalCost { get; set; }
    public int TotalPoints { get; set; }
    public int Level { get; set; }
}


