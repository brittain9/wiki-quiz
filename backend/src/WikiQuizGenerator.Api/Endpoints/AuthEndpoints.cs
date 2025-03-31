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
        var group = app.MapGroup("/api/auth")
                       .WithTags("Authentication");

        group.MapPost("register", async (RegisterRequest registerRequest, IAccountService accountService) =>
        {
            await accountService.RegisterAsync(registerRequest);

            return Results.Ok();
        });

        group.MapPost("login", async (LoginRequest loginRequest, IAccountService accountService) =>
        {
            await accountService.LoginAsync(loginRequest);

            return Results.Ok();
        });

        group.MapPost("/refresh", async (HttpContext httpContext, IAccountService accountService) =>
        {
            var refreshToken = httpContext.Request.Cookies["REFRESH_TOKEN"];

            await accountService.RefreshTokenAsync(refreshToken);

            return Results.Ok();
        });

        group.MapGet("login/google", ([FromQuery] string returnUrl, LinkGenerator linkGenerator,
            SignInManager<User> signManager, HttpContext context) =>
        {
            var properties = signManager.ConfigureExternalAuthenticationProperties("Google",
                linkGenerator.GetPathByName(context, "GoogleLoginCallback")
                + $"?returnUrl={returnUrl}");

            return Results.Challenge(properties, ["Google"]);
        });

        group.MapGet("login/google/callback", async ([FromQuery] string returnUrl,
            HttpContext context, IAccountService accountService) =>
        {
            var result = await context.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!result.Succeeded)
            {
                return Results.Redirect($"{returnUrl}?error=google_auth_failed");
            }

            try 
            {
                await accountService.LoginWithGoogleAsync(result.Principal);
                
                // Ensure we maintain the fromAuth parameter
                // If returnUrl already has query params, append to them
                var separator = returnUrl.Contains('?') ? '&' : '?';
                return Results.Redirect($"{returnUrl}{separator}fromAuth=true");
            }
            catch (Exception ex)
            {
                return Results.Redirect($"{returnUrl}?error={ex.Message.Replace(" ", "_")}");
            }

        }).WithName("GoogleLoginCallback");

        group.MapGet("/protected", () => Results.Ok(new { authenticated = true })).RequireAuthorization();
        
        group.MapGet("/movies", () => Results.Ok(new List<string> { "Matrix" })).RequireAuthorization();
        
        // Add endpoint to get current user info
        group.MapGet("/me", async (HttpContext context, UserManager<User> userManager) => 
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Unauthorized();
            }
            
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Results.NotFound();
            }
            
            return Results.Ok(new 
            { 
                id = user.Id,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName
            });
        }).RequireAuthorization();
        
        // Add a logout endpoint for proper cookie cleanup
        group.MapPost("/logout", (HttpContext context) => {
            // Clear auth cookies
            context.Response.Cookies.Delete("ACCESS_TOKEN");
            context.Response.Cookies.Delete("REFRESH_TOKEN");
            return Results.Ok();
        });
    }
}