namespace WikiQuizGenerator.Core.DomainObjects;

/// <summary>
/// Domain object representing a user - simplified for Cosmos DB
/// </summary>
public class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAtUtc { get; set; }
    public bool IsPremium { get; set; }
    public double TotalCost { get; set; }
    public int TotalPoints { get; set; }
    public int Level { get; set; } = 1;
    public bool EmailConfirmed { get; set; }
}