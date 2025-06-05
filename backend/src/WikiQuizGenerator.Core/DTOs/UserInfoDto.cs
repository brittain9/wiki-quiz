namespace WikiQuizGenerator.Core.DTOs;

public record UserInfoDto(Guid Id, string? Email, string? FirstName, string? LastName, int TotalPoints, int Level);
