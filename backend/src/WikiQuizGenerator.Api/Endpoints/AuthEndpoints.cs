using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http; // Needed for HttpContext, Results, LinkGenerator, IEndpointRouteBuilder
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc; // Needed for [FromQuery]
using WikiQuizGenerator.Core.Interfaces; // Assuming IAccountService is here
using WikiQuizGenerator.Core.Models; // Assuming User model is here
using WikiQuizGenerator.Core.Requests; // Assuming RegisterRequest, LoginRequest are here
using System.Security.Claims; // Needed for ClaimsPrincipal in LoginWithGoogleAsync
// Removed: Microsoft.Extensions.Logging (as it's not used in the new endpoints)
// Removed: UserInfoResponse DTO definition

namespace WikiQuizGenerator.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        // Group endpoints under /api/account
        // Updated the tag to "Account" to better reflect the new endpoints
        var group = app.MapGroup("/api/auth")
                       .WithTags("Authenication");

        // --- Register Endpoint ---
        group.MapPost("/register", async (RegisterRequest registerRequest, IAccountService accountService) =>
        {
            await accountService.RegisterAsync(registerRequest);
            return Results.Ok();
        })
        .AllowAnonymous(); // Typically, registration should be anonymous

        // --- Login Endpoint ---
        group.MapPost("/login", async (LoginRequest loginRequest, IAccountService accountService) =>
        {
            await accountService.LoginAsync(loginRequest);
            return Results.Ok();
        })
        .AllowAnonymous(); // Login endpoint should be anonymous

        // --- Refresh Token Endpoint ---
        group.MapPost("/refresh", async (HttpContext httpContext, IAccountService accountService) =>
        {
            // Extract refresh token from cookie
            var refreshToken = httpContext.Request.Cookies["REFRESH_TOKEN"];

            // Check if the token exists before calling the service
            if (string.IsNullOrEmpty(refreshToken))
            {
                // Or return Results.BadRequest("Refresh token not found.");
                return Results.Unauthorized();
            }

            await accountService.RefreshTokenAsync(refreshToken);
            return Results.Ok();
        })
        .AllowAnonymous(); // Refresh often needs to be anonymous as the access token might be expired

        // --- Google Login Initiation Endpoint ---
        group.MapGet("/login/google", ([FromQuery] string returnUrl, LinkGenerator linkGenerator,
            SignInManager<User> signInManager, HttpContext context) =>
        {
            // Validate returnUrl - Basic validation example
            if (string.IsNullOrEmpty(returnUrl) || !Uri.IsWellFormedUriString(returnUrl, UriKind.RelativeOrAbsolute))
            {
                // Decide on a default or return an error
                returnUrl = "/"; // Default to home page or a safe default
                // Optionally: return Results.BadRequest("Invalid return URL.");
            }

            // Construct the callback URL using the named route
            // Ensure the name "GoogleLoginCallback" matches the .WithName() call below
            var callbackPath = linkGenerator.GetPathByName(context, "GoogleLoginCallback", values: new { returnUrl });

            if (string.IsNullOrEmpty(callbackPath))
            {
                 // Log this error ideally
                 return Results.Problem("Could not generate Google callback path.", statusCode: StatusCodes.Status500InternalServerError);
            }

            // Configure properties for the external login provider (Google)
            var properties = signInManager.ConfigureExternalAuthenticationProperties(
                GoogleDefaults.AuthenticationScheme, // Use constant for provider name
                callbackPath // Pass the generated callback path
            );

            properties.AllowRefresh = true; // Allow token refresh if supported/needed

            // Initiate the challenge to redirect the user to Google
            return Results.Challenge(properties, [GoogleDefaults.AuthenticationScheme]);
        })
        .AllowAnonymous(); // Starting Google login must be anonymous

        // --- Google Login Callback Endpoint ---
        group.MapGet("/login/google/callback", async ([FromQuery] string returnUrl,
            HttpContext context, IAccountService accountService, SignInManager<User> signInManager) =>
        {
            // Validate returnUrl again (important for security)
             if (string.IsNullOrEmpty(returnUrl) || !Uri.IsWellFormedUriString(returnUrl, UriKind.RelativeOrAbsolute))
            {
                // Redirect to a safe default on invalid URL
                return Results.Redirect("/");
            }

            // Authenticate using the external cookie created by the challenge
            // This replaces GetExternalLoginInfoAsync in many minimal API scenarios
            var result = await context.AuthenticateAsync(IdentityConstants.ExternalScheme); // Use constant

            if (!result.Succeeded || result.Principal == null)
            {
                // Log error details if possible (result.Failure contains exception)
                // Redirect back with an error query parameter
                return Results.Redirect($"{returnUrl}?error=google_authentication_failed");
            }

            // Call the account service to handle the actual login/registration logic
            // Pass the ClaimsPrincipal obtained from the external authentication
            try
            {
                await accountService.LoginWithGoogleAsync(result.Principal);

                // Sign out the external cookie as it's no longer needed
                await context.SignOutAsync(IdentityConstants.ExternalScheme);

                // Redirect the user back to the original returnUrl upon successful login
                // Consider adding a query parameter to indicate success if needed by the frontend
                var separator = returnUrl.Contains('?') ? '&' : '?';
                return Results.Redirect($"{returnUrl}{separator}fromAuth=true");
            }
            catch (Exception ex)
            {
                // Log the exception ex
                // Clean up the external cookie even if there's an error
                await context.SignOutAsync(IdentityConstants.ExternalScheme);
                // Redirect with a generic error message
                 return Results.Redirect($"{returnUrl}?error=google_processing_error");
            }
        })
        .WithName("GoogleLoginCallback") // Name the route for LinkGenerator
        .AllowAnonymous(); // Callback endpoint is hit anonymously initially, relies on external cookie
    }
}