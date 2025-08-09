using System.Text.Json.Serialization;

namespace WikiQuizGenerator.Data.Cosmos.Models;

/// <summary>
/// Simplified User document - separate from Quiz for different access patterns
/// </summary>
public class User
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [JsonPropertyName("partitionKey")]
    public string PartitionKey { get; set; } = string.Empty; // Same as Id
    
    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;
    
    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("userName")]
    public string UserName { get; set; } = string.Empty;
    
    [JsonPropertyName("refreshToken")]
    public string? RefreshToken { get; set; }
    
    [JsonPropertyName("refreshTokenExpiresAtUtc")]
    public DateTime? RefreshTokenExpiresAtUtc { get; set; }
    
    [JsonPropertyName("isPremium")]
    public bool IsPremium { get; set; }
    
    [JsonPropertyName("totalCost")]
    public double TotalCost { get; set; }
    
    [JsonPropertyName("weeklyCost")]
    public double WeeklyCost { get; set; }
    
    [JsonPropertyName("totalPoints")]
    public int TotalPoints { get; set; }
    
    [JsonPropertyName("level")]
    public int Level { get; set; } = 1;
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = "user";
}