using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Models;
using WikiQuizGenerator.Core.Requests;
using System.Security.Claims;

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
                var properties = signInManager.ConfigureExternalAuthenticationProperties("Google",
                    linkGenerator.GetPathByName(context, "GoogleLoginCallback")
                    + $"?returnUrl={returnUrl}");

                return Results.Challenge(properties, ["Google"]);
            })
            .AllowAnonymous(); // Starting Google login must be anonymous

        // --- Google Login Callback Endpoint ---
        group.MapGet("/login/google/callback", async ([FromQuery] string returnUrl,
                HttpContext context, IAccountService accountService, SignInManager<User> signInManager) =>
            {
                var result = await context.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

                if (!result.Succeeded)
                {
                    return Results.Unauthorized();
                }

                await accountService.LoginWithGoogleAsync(result.Principal);

                return Results.Redirect(returnUrl);
            })
            .WithName("GoogleLoginCallback") // Name the route for LinkGenerator
            .AllowAnonymous(); // Callback endpoint is hit anonymously initially, relies on external cookie
        
        // --- Logout Endpoint ---
        group.MapPost("/logout", async (ClaimsPrincipal user, IAccountService accountService) =>
            {
                // Get user ID from claims
                if (!Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                {
                    return Results.Unauthorized();
                }
        
                await accountService.LogoutAsync(userId);
                return Results.Ok();
            })
            .RequireAuthorization(); // Only authenticated users can logout

        group.MapGet("/user", async (ClaimsPrincipal user, IAccountService accountService) =>
            {
                // Get user ID from claims
                if (!Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                {
                    return Results.Unauthorized();
                }
        
                var userInfo = await accountService.GetUserInfoAsync(userId);
                return Results.Ok(userInfo);
            })
            .RequireAuthorization(); // Only authenticated users can access their info
    }
}