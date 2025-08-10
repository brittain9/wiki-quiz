using System.Security.Claims;
using System.Security.Cryptography;
using WikiQuizGenerator.Core.DTOs;
using WikiQuizGenerator.Core.Exceptions;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Models;
using WikiQuizGenerator.Core.Requests;
using WikiQuizGenerator.Data.Cosmos.Repositories;
using Microsoft.Extensions.Logging;

namespace WikiQuizGenerator.Data.Cosmos.Services;

public class CosmosAccountService : IAccountService
{
    private readonly IAuthTokenProcessor _authTokenProcessor;
    private readonly CosmosUserRepository _userRepository;
    private readonly ILogger<CosmosAccountService> _logger;

    public CosmosAccountService(
        IAuthTokenProcessor authTokenProcessor, 
        CosmosUserRepository userRepository,
        ILogger<CosmosAccountService> logger)
    {
        _authTokenProcessor = authTokenProcessor;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task RegisterAsync(RegisterRequest registerRequest)
    {
        var existingUser = await _userRepository.GetUserByEmailAsync(registerRequest.Email);

        if (existingUser != null)
        {
            throw new UserAlreadyExistsException(email: registerRequest.Email);
        }

        var user = User.Create(registerRequest.Email, registerRequest.FirstName, registerRequest.LastName);
        user.PasswordHash = HashPassword(registerRequest.Password);

        try
        {
            await _userRepository.CreateUserAsync(user);
            _logger.LogInformation("Successfully registered user {Email}", registerRequest.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register user {Email}", registerRequest.Email);
            throw new RegistrationFailedException(new[] { "Failed to create user account" });
        }
    }

    public async Task LoginAsync(LoginRequest loginRequest)
    {
        var user = await _userRepository.GetUserByEmailAsync(loginRequest.Email);

        if (user == null || !VerifyPassword(loginRequest.Password, user.PasswordHash))
        {
            throw new LoginFailedException(loginRequest.Email);
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

    public async Task RefreshTokenAsync(string? refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            throw new RefreshTokenException("Refresh token is missing.");
        }

        var user = await _userRepository.GetUserByRefreshTokenAsync(refreshToken);

        if (user == null)
        {
            throw new RefreshTokenException("Unable to retrieve user for refresh token");
        }

        if (user.RefreshTokenExpiresAtUtc < DateTime.UtcNow)
        {
            throw new RefreshTokenException("Refresh token is expired.");
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
            
            var newUser = User.Create(email, firstName, lastName);
            newUser.EmailConfirmed = true;

            try
            {
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
        user.SecurityStamp = Guid.NewGuid().ToString(); // Update security stamp to invalidate existing tokens

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