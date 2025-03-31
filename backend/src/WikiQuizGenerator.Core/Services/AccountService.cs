using System; // Added for DateTime
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks; // Added for Task
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging; // Optional: For logging logout issues
using WikiQuizGenerator.Core.Exceptions;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Models;
using WikiQuizGenerator.Core.Requests;

namespace WikiQuizGenerator.Core.Services;

public class AccountService : IAccountService
{
    private readonly IAuthTokenProcessor _authTokenProcessor;
    private readonly UserManager<User> _userManager;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AccountService> _logger; // Optional: Added for logging

    public AccountService(
        IAuthTokenProcessor authTokenProcessor,
        UserManager<User> userManager,
        IUserRepository userRepository,
        ILogger<AccountService> logger) // Optional: Added logger
    {
        _authTokenProcessor = authTokenProcessor;
        _userManager = userManager;
        _userRepository = userRepository;
        _logger = logger; // Optional: Store logger instance
    }

    public async Task RegisterAsync(RegisterRequest registerRequest)
    {
        var userExists = await _userManager.FindByEmailAsync(registerRequest.Email) != null;

        if (userExists)
        {
            throw new UserAlreadyExistsException(email: registerRequest.Email);
        }

        // Ensure Username is set if required by Identity defaults
        var user = User.Create(registerRequest.Email, registerRequest.FirstName, registerRequest.LastName);
        user.UserName ??= registerRequest.Email; // Set UserName if Create doesn't

        // Don't set PasswordHash directly, use CreateAsync with password
        // user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, registerRequest.Password);
        // var result = await _userManager.CreateAsync(user);

        // Use UserManager to create user with password which handles hashing
        var result = await _userManager.CreateAsync(user, registerRequest.Password);

        if (!result.Succeeded)
        {
            throw new RegistrationFailedException(result.Errors.Select(x => x.Description));
        }
    }

    public async Task LoginAsync(LoginRequest loginRequest)
    {
        var user = await _userManager.FindByEmailAsync(loginRequest.Email);

        // Use SignInManager.CheckPasswordSignInAsync for lockout checks etc.
        // Or keep CheckPasswordAsync if simplicity is preferred and lockout isn't needed here.
        if (user == null || !await _userManager.CheckPasswordAsync(user, loginRequest.Password))
        {
            throw new LoginFailedException(loginRequest.Email);
        }

        // --- Token Generation and Cookie Writing ---
        await GenerateAndSetTokensAsync(user);
        // Note: ASP.NET Core Identity's SignInManager should ideally be used here
        // to set the primary authentication cookie if you rely on Identity's auth scheme.
        // e.g., await _signInManager.SignInAsync(user, isPersistent: true);
        // Your current approach seems to bypass Identity's cookie scheme in favour of custom JWT cookies.
    }

    public async Task RefreshTokenAsync(string? refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            throw new RefreshTokenException("Refresh token is missing.");
        }

        // Consider adding index on RefreshToken column for performance
        var user = await _userRepository.GetUserByRefreshTokenAsync(refreshToken);

        if (user == null)
        {
            // Security: If a refresh token is used but doesn't match a user,
            // potentially invalidate all tokens for the user it *might* belong to,
            // or log it as suspicious activity.
            throw new RefreshTokenException("Invalid refresh token.");
        }

        if (user.RefreshToken != refreshToken)
        {
            // Security: If the token matches a user BUT the stored token is different
            // (e.g., already rotated), this could be a stolen/replayed token.
            // Consider invalidating all sessions for this user.
             _logger.LogWarning("Potential refresh token reuse detected for user {UserId}.", user.Id);
            // Optionally clear stored token: user.RefreshToken = null; await _userManager.UpdateAsync(user);
            throw new RefreshTokenException("Invalid refresh token.");
        }

        if (user.RefreshTokenExpiresAtUtc < DateTime.UtcNow)
        {
            throw new RefreshTokenException("Refresh token has expired.");
        }

        // --- Token Generation and Cookie Writing (Rotation) ---
        await GenerateAndSetTokensAsync(user);
    }

     public async Task LoginWithGoogleAsync(ExternalLoginInfo info) // Changed parameter type
    {
        if (info == null)
        {
            throw new ExternalLoginProviderException("Google", "ExternalLoginInfo is null.");
        }

        // Attempt to sign in the user directly with the external login provider info
        // This links the external login to an existing user if possible.
        // var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
        // if (signInResult.Succeeded) {
        //     var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
        //     await GenerateAndSetTokensAsync(user);
        //     return; // Already logged in and tokens set
        // }
        // if (signInResult.IsLockedOut) { throw new LoginFailedException("Account locked out."); }
        // if (signInResult.IsNotAllowed) { throw new LoginFailedException("Login not allowed."); }

        // If SignInManager isn't used or fails (e.g., user not yet linked/created):
        // Get user details from claims
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email))
        {
            throw new ExternalLoginProviderException("Google", "Email claim not found.");
        }

        var user = await _userManager.FindByEmailAsync(email);

        if (user == null) // User doesn't exist, create a new one
        {
            user = new User
            {
                UserName = email, // Ensure UserName is set
                Email = email,
                FirstName = info.Principal.FindFirstValue(ClaimTypes.GivenName) ?? string.Empty,
                LastName = info.Principal.FindFirstValue(ClaimTypes.Surname) ?? string.Empty,
                EmailConfirmed = true // Assume email from Google is verified
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                throw new RegistrationFailedException(createResult.Errors.Select(e => e.Description));
            }
        }

        // Add the external login to the user (idempotent)
        var addLoginResult = await _userManager.AddLoginAsync(user, info);
        if (!addLoginResult.Succeeded)
        {
            // Log or handle cases where adding the login fails (e.g., already exists but linked to another user?)
             _logger.LogError("Failed to add Google login for user {UserId}: {Errors}", user.Id, string.Join(", ", addLoginResult.Errors.Select(e => e.Description)));
             // Decide if this is a critical failure or if you can proceed. Often you can proceed.
             // throw new ExternalLoginProviderException("Google", $"Failed to add login: {string.Join(", ", addLoginResult.Errors.Select(e => e.Description))}");
        }

        // --- Token Generation and Cookie Writing ---
        await GenerateAndSetTokensAsync(user);
    }

    // --- NEW Logout Method ---
    public async Task LogoutAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user != null)
        {
            // Invalidate the stored refresh token
            user.RefreshToken = null;
            user.RefreshTokenExpiresAtUtc = null;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                // Log the failure but generally proceed with cookie clearing
                _logger.LogWarning("Failed to clear refresh token for user {UserId} during logout. Errors: {Errors}",
                    userId, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
             _logger.LogWarning("User {UserId} not found during logout token invalidation.", userId);
            // User not found, but still attempt to clear cookies client-side
        }

        // Clear the custom authentication cookies using the processor
        // Assumes IAuthTokenProcessor has access to HttpContext to delete response cookies
        _authTokenProcessor.DeleteAuthTokenCookie("ACCESS_TOKEN");
        _authTokenProcessor.DeleteAuthTokenCookie("REFRESH_TOKEN");

        // Note: This method DOES NOT call SignInManager.SignOutAsync().
        // That should be done in the *endpoint* handler to clear the primary
        // ASP.NET Core Identity authentication cookie, if you are using it.
    }


    // Helper method to consolidate token generation and cookie writing
    private async Task GenerateAndSetTokensAsync(User user)
    {
        var (jwtToken, expirationDateInUtc) = _authTokenProcessor.GenerateJwtToken(user);
        var refreshTokenValue = _authTokenProcessor.GenerateRefreshToken();
        var refreshTokenExpirationDateInUtc = DateTime.UtcNow.AddDays(7); // Consider making duration configurable

        user.RefreshToken = refreshTokenValue;
        user.RefreshTokenExpiresAtUtc = refreshTokenExpirationDateInUtc;

        // Update the user BEFORE writing cookies, in case update fails.
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            // Log the error details
            _logger.LogError("Failed to update user {UserId} with new refresh token. Errors: {Errors}",
                user.Id, string.Join(", ", updateResult.Errors.Select(e => e.Description)));
            // Throw an exception to prevent potentially inconsistent state (token issued but not saved)
            throw new Exception($"Failed to save refresh token for user {user.Email}.");
        }

        _authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie("ACCESS_TOKEN", jwtToken, expirationDateInUtc);
        _authTokenProcessor.WriteAuthTokenAsHttpOnlyCookie("REFRESH_TOKEN", user.RefreshToken, refreshTokenExpirationDateInUtc);
    }
}