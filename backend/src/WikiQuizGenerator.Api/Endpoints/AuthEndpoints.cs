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
                       .WithTags("Authentication")
                       // Optional: Apply authorization requirement to the whole group if most endpoints need it
                       // .RequireAuthorization()
                       ; 

        // --- Registration, Login, Refresh ---
        // (Keep existing registration, login, refresh, google endpoints as they are,
        // assuming IAccountService handles cookie setting correctly for login/refresh)

        group.MapPost("register", async (RegisterRequest registerRequest, IAccountService accountService) =>
        {
            // Consider adding error handling/returning specific results (e.g., BadRequest on failure)
            await accountService.RegisterAsync(registerRequest);
            return Results.Ok();
        })
        .AllowAnonymous(); // Explicitly allow anonymous access if group requires authorization

        group.MapPost("login", async (LoginRequest loginRequest, IAccountService accountService, HttpContext context) =>
        {
            // Assuming LoginAsync sets necessary cookies (e.g., application cookie via SignInManager, refresh token cookie)
            // Consider returning user info or confirmation instead of just Ok()
            // Also handle login failures (e.g., return Results.Unauthorized())
            await accountService.LoginAsync(loginRequest); // Ensure this internally calls SignInManager.PasswordSignInAsync etc.
            return Results.Ok();
        })
        .AllowAnonymous(); // Explicitly allow anonymous access

        group.MapPost("/refresh", async (HttpContext httpContext, IAccountService accountService) =>
        {
            var refreshToken = httpContext.Request.Cookies["REFRESH_TOKEN"]; // Still relies on specific cookie name

            if (string.IsNullOrEmpty(refreshToken))
            {
                return Results.BadRequest("Refresh token not found.");
            }
            // Ensure RefreshTokenAsync handles validation, generates new tokens (access + refresh),
            // sets new cookies (including HttpOnly, Secure, SameSite), and potentially revokes the old refresh token.
            // Handle failures (e.g., invalid token) -> Results.Unauthorized()
            await accountService.RefreshTokenAsync(refreshToken);
            return Results.Ok();
        }); // This likely needs authorization OR a valid refresh token

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
            HttpContext context, SignInManager<User> signInManager, IAccountService accountService) =>
        {
            // Basic validation for returnUrl
            returnUrl ??= "/"; // Default redirect location if none provided
            if (!Uri.IsWellFormedUriString(returnUrl, UriKind.RelativeOrAbsolute)) {
                returnUrl = "/"; // Fallback to safe default on invalid URL
            }

            // Use SignInManager to get external login info
            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                // Log error details if possible
                return Results.Redirect($"{returnUrl}?error=external_login_failed");
            }

            try
            {
                // Let IAccountService handle the logic of signing in or registering the external user
                // This service should internally use SignInManager.ExternalLoginSignInAsync or create/link the user
                await accountService.LoginWithGoogleAsync(info); // Pass ExternalLoginInfo

                // Redirect after successful login
                var separator = returnUrl.Contains('?') ? '&' : '?';
                return Results.Redirect($"{returnUrl}{separator}fromAuth=true");
            }
            catch (Exception ex)
            {
                // Log the exception ex
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
            // Check if we're authenticated and log identity info
            logger.LogInformation("User authenticated: {IsAuthenticated}, Claims: {ClaimsCount}", 
                claimsPrincipal.Identity?.IsAuthenticated, 
                claimsPrincipal.Claims?.Count());

            if (!claimsPrincipal.Identity?.IsAuthenticated ?? true)
            {
                logger.LogWarning("User not authenticated");
                return Results.Unauthorized();
            }

            // Log all claims to help debug
            foreach (var claim in claimsPrincipal.Claims)
            {
                logger.LogInformation("Claim: {Type} = {Value}", claim.Type, claim.Value);
            }

            // Log the available cookies
            logger.LogInformation("Cookies: {Cookies}", string.Join(", ", 
                httpContext.Request.Cookies.Select(c => $"{c.Key}={c.Value.Substring(0, Math.Min(5, c.Value.Length))}...")));

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


        // --- Optimized /logout Endpoint ---
        group.MapPost("/logout", async (SignInManager<User> signInManager, IAccountService accountService, ClaimsPrincipal claimsPrincipal, HttpContext context) => {

            // Optional: Invalidate the refresh token if your IAccountService manages them server-side
            // You might need to retrieve the token used (e.g., from a claim or a cookie)
            // or identify the session based on the user.
            // Example:
            // var userId = userManager.GetUserId(claimsPrincipal); // Get user ID if needed for revocation
            // var refreshToken = context.Request.Cookies["REFRESH_TOKEN"]; // If you need the token value
            // if(!string.IsNullOrEmpty(refreshToken)) {
            //      await accountService.RevokeRefreshTokenAsync(refreshToken, userId); // Your service method
            // }

            // Use SignInManager to properly sign out
            // This clears the Identity.Application cookie
            await signInManager.SignOutAsync();

            // Optional: Manually delete custom cookies if SignInManager doesn't handle them
            // Only do this if IAccountService sets these cookies *outside* of Identity's control
            // context.Response.Cookies.Delete("REFRESH_TOKEN"); // Example if REFRESH_TOKEN is managed separately

            return Results.Ok("Logged out successfully.");
            // Or use Results.NoContent();
        })
        .RequireAuthorization(); // User must be logged in to log out
    }
}