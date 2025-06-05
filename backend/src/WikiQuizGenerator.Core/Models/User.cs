using Microsoft.AspNetCore.Identity;

namespace WikiQuizGenerator.Core.Models;

public class User : IdentityUser<Guid>
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiresAtUtc { get; set; }
    public bool isPremium { get; set; } // dont limit cost of premium users
    public double TotalCost { get; set; }
    public double WeeklyCost { get; set; } // For rate limiting based on weekly usage.
    public int TotalPoints { get; set; } = 0;
    public int Level { get; set; } = 1;

    public static User Create(string email, string firstName, string lastName)
    {
        return new User
        {
            Email = email,
            UserName = email,
            FirstName = firstName,
            LastName = lastName
        };
    }
    
    public override string ToString()
    {
        return FirstName + " " + LastName;
    }
}
