using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Models;
using WikiQuizGenerator.Core.Requests;

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
                return Results.Unauthorized();
            }

            await accountService.LoginWithGoogleAsync(result.Principal);

            return Results.Redirect(returnUrl);

        }).WithName("GoogleLoginCallback");

        group.MapGet("/api/movies", () => Results.Ok(new List<string> { "Matrix" })).RequireAuthorization();
    }
}