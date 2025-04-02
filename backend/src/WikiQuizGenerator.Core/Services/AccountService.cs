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
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<AccountService> _logger;

    public AccountService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IUserRepository userRepository,
        ILogger<AccountService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _userRepository = userRepository;
        _logger = logger;
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

        if (user == null)
        {
            throw new LoginFailedException(loginRequest.Email);
        }

        var signInResult = await _signInManager.PasswordSignInAsync(
            user, 
            loginRequest.Password, 
            isPersistent: true, 
            lockoutOnFailure: false);
            
        if (!signInResult.Succeeded)
        {
            throw new LoginFailedException(loginRequest.Email);
        }
        
        _logger.LogInformation("User {Email} logged in successfully", loginRequest.Email);
    }

    public async Task LoginWithGoogleAsync(ExternalLoginInfo info)
    {
        _logger.LogInformation("LoginWithGoogleAsync called with provider {Provider}", info?.LoginProvider ?? "unknown");
        
        if (info == null)
        {
            _logger.LogError("External login info is null");
            throw new ExternalLoginProviderException("Google", "ExternalLoginInfo is null.");
        }

        // Log all claims for debugging
        _logger.LogDebug("Google login claims:");
        foreach (var claim in info.Principal.Claims)
        {
            _logger.LogDebug("Claim: {Type} = {Value}", claim.Type, claim.Value);
        }
        
        // Get user details from claims
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email))
        {
            // Try alternate claim types
            email = info.Principal.FindFirstValue("email");
        }
        
        _logger.LogInformation("Email from Google claim: {Email}", email ?? "not found");
        
        if (string.IsNullOrEmpty(email))
        {
            _logger.LogError("Email claim not found in Google login info");
            throw new ExternalLoginProviderException("Google", "Email claim not found in claims.");
        }

        // Try to find existing user by email
        var user = await _userManager.FindByEmailAsync(email);
        _logger.LogInformation("User with email {Email} exists: {Exists}", email, user != null);

        if (user == null) // User doesn't exist, create a new one
        {
            _logger.LogInformation("Creating new user with email {Email}", email);
            
            var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
            var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname);
            
            _logger.LogDebug("Creating user with firstName: {FirstName}, lastName: {LastName}", 
                firstName ?? "empty", lastName ?? "empty");
            
            user = new User
            {
                UserName = email, // Ensure UserName is set
                Email = email,
                FirstName = firstName ?? string.Empty,
                LastName = lastName ?? string.Empty,
                EmailConfirmed = true // Assume email from Google is verified
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                _logger.LogError("Failed to create user with email {Email}. Errors: {Errors}", 
                    email, string.Join(", ", createResult.Errors.Select(e => e.Description)));
                throw new RegistrationFailedException(createResult.Errors.Select(e => e.Description));
            }
            
            _logger.LogInformation("Successfully created new user with ID {UserId} and email {Email}", 
                user.Id, user.Email);
                
            // Retrieve the user after creation to ensure we have the latest version
            user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogError("Failed to retrieve newly created user with email {Email}", email);
                throw new ExternalLoginProviderException("Google", "User was created but could not be retrieved.");
            }
        }

        // Add the external login to the user (idempotent)
        _logger.LogInformation("Adding Google login to user {UserId} with provider key {ProviderKey}", 
            user.Id, info.ProviderKey);
            
        var addLoginResult = await _userManager.AddLoginAsync(user, info);
        if (!addLoginResult.Succeeded)
        {
            // Log or handle cases where adding the login fails
            _logger.LogError("Failed to add Google login for user {UserId}: {Errors}", 
                user.Id, string.Join(", ", addLoginResult.Errors.Select(e => e.Description)));
        }

        // Sign in the user with Identity cookies
        _logger.LogInformation("Signing in user {UserId} with external login", user.Id);
        try
        {
            await _signInManager.SignInAsync(user, isPersistent: true);
            _logger.LogInformation("Login with Google completed successfully for user {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sign in user {UserId}", user.Id);
            throw;
        }
    }

    public async Task LogoutAsync(Guid userId)
    {
        _logger.LogInformation("Logging out user {UserId}", userId);
        
        // SignOut using SignInManager - this handles the cookie cleanup
        await _signInManager.SignOutAsync();
        
        _logger.LogInformation("User {UserId} successfully logged out", userId);
    }
}