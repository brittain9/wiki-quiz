using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Models;
using WikiQuizGenerator.Core.Requests;
using System.Security.Claims;
using Microsoft.AspNetCore.Http; // Needed for Results and StatusCodes
using Microsoft.Extensions.Logging;

namespace WikiQuizGenerator.Api.Endpoints;

// Define a DTO for the /me endpoint response
public record UserInfoResponse(Guid Id, string? Email, string? FirstName, string? LastName);

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
                       .WithTags("Authentication");

        // --- Registration, Login ---
        group.MapPost("register", async (RegisterRequest registerRequest, IAccountService accountService) =>
        {
            // Consider adding error handling/returning specific results (e.g., BadRequest on failure)
            await accountService.RegisterAsync(registerRequest);
            return Results.Ok();
        })
        .AllowAnonymous(); // Explicitly allow anonymous access

        group.MapPost("login", async (LoginRequest loginRequest, IAccountService accountService, HttpContext context) =>
        {
            await accountService.LoginAsync(loginRequest);
            return Results.Ok();
        })
        .AllowAnonymous(); // Explicitly allow anonymous access

        // --- Google Login ---
        group.MapGet("login/google", ([FromQuery] string? returnUrl, LinkGenerator linkGenerator,
            SignInManager<User> signInManager, HttpContext context) =>
        {
            // Ensure returnUrl is validated to prevent open redirect vulnerabilities
            var callbackUrl = linkGenerator.GetUriByName(context, "GoogleLoginCallback", values: new { returnUrl });
            if (string.IsNullOrEmpty(callbackUrl))
            {
                // Handle error: Callback route not found
                return Results.Problem("Could not generate Google callback URL.", statusCode: StatusCodes.Status500InternalServerError);
            }
            
            var properties = signInManager.ConfigureExternalAuthenticationProperties(GoogleDefaults.AuthenticationScheme, callbackUrl);
            properties.AllowRefresh = true; // Optional: Allow refresh tokens if needed
            return Results.Challenge(properties, [GoogleDefaults.AuthenticationScheme]);
        })
        .AllowAnonymous(); // Explicitly allow anonymous access

        group.MapGet("login/google/callback", async ([FromQuery] string? returnUrl,
            HttpContext context, SignInManager<User> signInManager, IAccountService accountService, ILogger<Program> logger) =>
        {
            logger.LogInformation("Google callback endpoint hit with returnUrl: {ReturnUrl}", returnUrl ?? "null");
            
            // Basic validation for returnUrl
            returnUrl ??= "/"; // Default redirect location if none provided
            if (!Uri.IsWellFormedUriString(returnUrl, UriKind.RelativeOrAbsolute)) {
                returnUrl = "/"; // Fallback to safe default on invalid URL
            }

            // Use SignInManager to get external login info
            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                logger.LogError("Failed to get external login info from SignInManager");
                // Log error details if possible
                return Results.Redirect($"{returnUrl}?error=external_login_failed");
            }
            
            logger.LogInformation("Retrieved external login info for provider: {Provider}, with claims count: {ClaimsCount}", 
                info.LoginProvider, info.Principal?.Claims?.Count() ?? 0);

            try
            {
                // Let IAccountService handle the logic of signing in or registering the external user
                logger.LogInformation("Calling accountService.LoginWithGoogleAsync");
                await accountService.LoginWithGoogleAsync(info); // Pass ExternalLoginInfo
                logger.LogInformation("Successfully authenticated with Google");

                // Redirect after successful login
                var separator = returnUrl.Contains('?') ? '&' : '?';
                return Results.Redirect($"{returnUrl}{separator}fromAuth=true");
            }
            catch (Exception ex)
            {
                // Log the exception ex
                logger.LogError(ex, "Error processing Google login callback");
                // Consider more specific error handling/messages
                return Results.Redirect($"{returnUrl}?error=google_login_processing_error");
            }
            finally
            {
                // Clean up temporary external cookie
                await context.SignOutAsync(IdentityConstants.ExternalScheme);
            }
        }).WithName("GoogleLoginCallback")
          .AllowAnonymous(); // Explicitly allow anonymous access

        // --- Optimized /me Endpoint ---
        group.MapGet("/me", async (ClaimsPrincipal claimsPrincipal, UserManager<User> userManager, HttpContext httpContext, ILogger<Program> logger) =>
        {
            if (!claimsPrincipal.Identity?.IsAuthenticated ?? true)
            {
                logger.LogInformation("Unauthenticated request to /me endpoint");
                return Results.Unauthorized();
            }

            try
            {
                // Use FindByEmailAsync instead of GetUserAsync to work around claim format issues
                var emailClaim = claimsPrincipal.FindFirst(ClaimTypes.Email) ?? 
                                claimsPrincipal.FindFirst("email");
                
                if (emailClaim == null)
                {
                    logger.LogWarning("No email claim found");
                    return Results.Unauthorized();
                }
                
                var email = emailClaim.Value;
                logger.LogInformation("Found email claim: {Email}", email);
                
                var user = await userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    // This case should be rare if RequireAuthorization works, but handles edge cases (e.g., user deleted after token issuance)
                    logger.LogWarning("User with email {Email} not found in database", email);
                    return Results.Unauthorized();
                }

                logger.LogInformation("User found: {UserId}, {Email}", user.Id, user.Email);

                // Return the DTO
                return Results.Ok(new UserInfoResponse(
                    user.Id,
                    user.Email,
                    user.FirstName,
                    user.LastName
                ));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in /me endpoint");
                return Results.Problem("An error occurred while retrieving user information.");
            }
        })
        .RequireAuthorization(); // Ensures only authenticated users can access

        // --- Logout Endpoint ---
        group.MapPost("/logout", async (IAccountService accountService, ClaimsPrincipal claimsPrincipal, ILogger<Program> logger) => 
        {
            if (!claimsPrincipal.Identity?.IsAuthenticated ?? true)
            {
                return Results.Ok(); // Already logged out
            }

            var nameId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(nameId) || !Guid.TryParse(nameId, out var userId))
            {
                logger.LogWarning("No valid user ID found in claims during logout");
                return Results.Ok(); // No valid user ID, but still OK response
            }

            await accountService.LogoutAsync(userId);
            return Results.Ok();
        })
        .RequireAuthorization(); // User must be logged in to log out

        // Debug endpoints
        group.MapGet("/debug/auth-status", (ClaimsPrincipal user, HttpContext context, ILogger<Program> logger) =>
        {
            // Log request details
            logger.LogInformation("Auth status check - IsAuthenticated: {IsAuthenticated}", 
                user.Identity?.IsAuthenticated ?? false);
            
            // Log all headers
            foreach (var header in context.Request.Headers)
            {
                logger.LogDebug("Header: {Key} = {Value}", header.Key, header.Value);
            }
            
            // Log all cookies
            logger.LogDebug("Cookies: {Count}", context.Request.Cookies.Count);
            foreach (var cookie in context.Request.Cookies)
            {
                // Don't log the full value of sensitive cookies
                var isSensitive = cookie.Key.Contains("token", StringComparison.OrdinalIgnoreCase) ||
                                 cookie.Key.Contains("auth", StringComparison.OrdinalIgnoreCase);
                
                var displayValue = isSensitive 
                    ? $"[Redacted - Length: {cookie.Value.Length}]" 
                    : cookie.Value;
                    
                logger.LogDebug("Cookie: {Key} = {Value}", cookie.Key, displayValue);
            }
            
            // Log all claims
            var claims = user.Claims.Select(c => new { c.Type, c.Value }).ToList();
            logger.LogDebug("Claims: {Count}", claims.Count);
            foreach (var claim in claims)
            {
                logger.LogDebug("Claim: {Type} = {Value}", claim.Type, claim.Value);
            }
            
            return Results.Ok(new 
            { 
                isAuthenticated = user.Identity?.IsAuthenticated ?? false,
                authType = user.Identity?.AuthenticationType,
                claimsCount = claims.Count,
                hasClaims = new
                {
                    email = user.HasClaim(c => c.Type == ClaimTypes.Email || c.Type == "email"),
                    nameIdentifier = user.HasClaim(c => c.Type == ClaimTypes.NameIdentifier),
                    name = user.HasClaim(c => c.Type == ClaimTypes.Name)
                },
                cookies = context.Request.Cookies.Keys.ToList()
            });
        })
        .AllowAnonymous();

        group.MapGet("/debug/create-test-user", async (UserManager<User> userManager, SignInManager<User> signInManager, HttpContext context, ILogger<Program> logger) =>
        {
            try {
                // Check if test user exists
                string email = "test@example.com";
                var existingUser = await userManager.FindByEmailAsync(email);
                
                if (existingUser != null) {
                    // Sign in existing user
                    await signInManager.SignInAsync(existingUser, isPersistent: true);
                    return Results.Ok(new { message = "Existing test user logged in", userId = existingUser.Id });
                }
                
                // Create new test user
                var user = new User
                {
                    UserName = email,
                    Email = email,
                    FirstName = "Test",
                    LastName = "User",
                    EmailConfirmed = true
                };
                
                var result = await userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Results.BadRequest(new { message = "Failed to create test user", errors });
                }
                
                // Sign in the new user
                await signInManager.SignInAsync(user, isPersistent: true);
                
                return Results.Ok(new { message = "Test user created and logged in", userId = user.Id });
            }
            catch (Exception ex) {
                logger.LogError(ex, "Error creating test user");
                return Results.Problem("Error creating test user: " + ex.Message);
            }
        })
        .AllowAnonymous();
    }
}