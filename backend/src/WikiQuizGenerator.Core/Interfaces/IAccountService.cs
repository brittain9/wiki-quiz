using System.Security.Claims;
using WikiQuizGenerator.Core.Requests;
using WikiQuizGenerator.Core.DTOs;

namespace WikiQuizGenerator.Core.Interfaces;

public interface IAccountService
{
    Task RegisterAsync(RegisterRequest registerRequest);
    Task LoginAsync(LoginRequest loginRequest);
    Task RefreshTokenAsync(string? refreshToken);
    Task LoginWithGoogleAsync(ClaimsPrincipal? claimsPrincipal);
    Task LogoutAsync(Guid userId);
    Task<UserInfoDto> GetUserInfoAsync(Guid userId);

}