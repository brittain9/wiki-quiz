using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Requests;
using System.Security.Claims;

namespace WikiQuizGenerator.Api.Endpoints;
public static class AuthEndpoints
{

    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authenication");

        group.MapPost("/register", async (RegisterRequest registerRequest, IAccountService accountService) =>
            {
                await accountService.RegisterAsync(registerRequest);
                return Results.Ok();
            })
            .AllowAnonymous();

        group.MapPost("/login", async (LoginRequest loginRequest, IAccountService accountService) =>
            {
                await accountService.LoginAsync(loginRequest);
                return Results.Ok();
            })
            .AllowAnonymous();

        group.MapPost("/refresh", async (HttpContext httpContext, IAccountService accountService) =>
            {
                // Extract refresh token from cookie
                var refreshToken = httpContext.Request.Cookies["REFRESH_TOKEN"];

                // Check if the token exists before calling the service
                if (string.IsNullOrEmpty(refreshToken))
                {
                    return Results.Unauthorized();
                }

                await accountService.RefreshTokenAsync(refreshToken);
                return Results.Ok();
            })
            .AllowAnonymous();

        group.MapGet("/login/google", ([FromQuery] string returnUrl, LinkGenerator linkGenerator,
                HttpContext context, ILogger<Program> logger) =>
            {
                // Get the callback path
                var callbackPath = linkGenerator.GetPathByName(context, "GoogleLoginCallback");
                
                var scheme = context.Request.Scheme;
                var host = context.Request.Host.Value;

                var callbackUrl = $"{scheme}://{host}{callbackPath}?returnUrl={returnUrl}";
                logger.LogInformation("Using callback URL: {CallbackUrl}", callbackUrl);
                
                var properties = new AuthenticationProperties
                {
                    RedirectUri = callbackUrl
                };

                return Results.Challenge(properties, ["Google"]);
            })
            .AllowAnonymous();

        group.MapGet("/login/google/callback", async ([FromQuery] string returnUrl,
                HttpContext context, IAccountService accountService) =>
            {
                var result = await context.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

                if (!result.Succeeded)
                {
                    return Results.Unauthorized();
                }

                await accountService.LoginWithGoogleAsync(result.Principal);

                return Results.Redirect(returnUrl);
            })
            .WithName("GoogleLoginCallback")
            .AllowAnonymous();
        
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
            .RequireAuthorization();

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
            .RequireAuthorization();
    }
}