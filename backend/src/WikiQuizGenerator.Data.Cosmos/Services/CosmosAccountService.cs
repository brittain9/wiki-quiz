using System.Security.Claims;
using System.Security.Cryptography;
using WikiQuizGenerator.Core.DTOs;
using WikiQuizGenerator.Core.Exceptions;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.DomainObjects;
using WikiQuizGenerator.Data.Cosmos.Repositories;
using Microsoft.Extensions.Logging;

namespace WikiQuizGenerator.Data.Cosmos.Services;

public class CosmosAccountService : IAccountService
{
    private readonly IAuthTokenService _authTokenProcessor;
    private readonly CosmosUserRepository _userRepository;
    private readonly ILogger<CosmosAccountService> _logger;

    public CosmosAccountService(
        IAuthTokenService authTokenProcessor, 
        CosmosUserRepository userRepository,
        ILogger<CosmosAccountService> logger)
    {
        _authTokenProcessor = authTokenProcessor;
        _userRepository = userRepository;
        _logger = logger;
    }

    // Username/password and refresh flows removed. Only Google OAuth is supported.

    public async Task LoginWithGoogleAsync(ClaimsPrincipal? claimsPrincipal)
    {
        if (claimsPrincipal == null)
        {
            throw new ExternalLoginProviderException("Google", "ClaimsPrincipal is null");
        }
        
        var email = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;

        if (email == null)
        {
            throw new ExternalLoginProviderException("Google", "Email is null");
        }

        var user = await _userRepository.GetUserByEmailAsync(email);

        if (user == null)
        {
            var firstName = claimsPrincipal.FindFirst(ClaimTypes.GivenName)?.Value ?? string.Empty;
            var lastName = claimsPrincipal.FindFirst(ClaimTypes.Surname)?.Value ?? string.Empty;
            
            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                UserName = email,
                EmailConfirmed = true
            };

            try
            {
                newUser.Id = Guid.NewGuid();
                user = await _userRepository.CreateUserAsync(newUser);
                _logger.LogInformation("Created new user from Google login {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create user from Google login {Email}", email);
                throw new ExternalLoginProviderException("Google", "Unable to create user account");
            }
        }
        
        var (jwtToken, expirationDateInUtc) = _authTokenProcessor.GenerateJwtToken(user);
        var refreshTokenValue = _authTokenProcessor.GenerateRefreshToken();

        var refreshTokenExpirationDateInUtc = DateTime.UtcNow.AddDays(7);

        user.RefreshToken = refreshTokenValue;
        user.RefreshTokenExpiresAtUtc = refreshTokenExpirationDateInUtc;

        await _userRepository.UpdateUserAsync(user);
        
        _authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie("ACCESS_TOKEN", jwtToken, expirationDateInUtc);
        _authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie("REFRESH_TOKEN", user.RefreshToken, refreshTokenExpirationDateInUtc);
    }
    
    public async Task LogoutAsync(Guid userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        // Clear the refresh token in the database
        user.RefreshToken = null;
        user.RefreshTokenExpiresAtUtc = null;

        await _userRepository.UpdateUserAsync(user);

        // Clear the auth cookies
        _authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie("ACCESS_TOKEN", "", DateTime.UtcNow.AddDays(-1));
        _authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie("REFRESH_TOKEN", "", DateTime.UtcNow.AddDays(-1));
    }

    public async Task<UserInfoDto> GetUserInfoAsync(Guid userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        return new UserInfoDto
        (
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.TotalPoints,
            user.Level
        );
    }

    private static string HashPassword(string password)
    {
        // Simple password hashing - in production, consider using BCrypt or similar
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password + "WikiQuizSalt"));
        return Convert.ToBase64String(hashedBytes);
    }

    private static bool VerifyPassword(string password, string? hashedPassword)
    {
        if (string.IsNullOrEmpty(hashedPassword))
            return false;
            
        var computedHash = HashPassword(password);
        return computedHash == hashedPassword;
    }
}