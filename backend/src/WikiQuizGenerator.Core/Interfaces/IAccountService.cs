using System.Security.Claims;
using WikiQuizGenerator.Core.DTOs;

namespace WikiQuizGenerator.Core.Interfaces;

public interface IAccountService
{
    Task LoginWithGoogleAsync(ClaimsPrincipal? claimsPrincipal);
    Task LogoutAsync(Guid userId);
    Task<UserInfoDto> GetUserInfoAsync(Guid userId);
}