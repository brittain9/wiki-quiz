using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using WikiQuizGenerator.Core.Interfaces;
using System.Security.Claims;

namespace WikiQuizGenerator.Api.Endpoints;
public static class AuthEndpoints
{

    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authenication");

        group.MapGet("/login/google", ([FromQuery] string returnUrl, HttpContext context, ILogger<Program> logger, IHostEnvironment env) =>
            {
                var scheme = env.IsDevelopment() ? context.Request.Scheme : "https";
                var host = context.Request.Host.Value;

                // Use the standard Google OAuth callback path that matches Google Cloud Console
                var callbackUrl = $"{scheme}://{host}/signin-google?returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}";
                logger.LogInformation("Using callback URL: {CallbackUrl}", callbackUrl);
                
                var properties = new AuthenticationProperties
                {
                    RedirectUri = callbackUrl
                };

                return Results.Challenge(properties, ["Google"]);
            })
            .AllowAnonymous();

        // Standard Google OAuth callback endpoint (matches Google Cloud Console redirect URI)
        // This needs to be at root level, not under /api/auth
        app.MapGet("/signin-google", async ([FromQuery] string returnUrl,
                HttpContext context, IAccountService accountService, IConfiguration configuration) =>
            {
                var result = await context.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

                if (!result.Succeeded)
                {
                    return Results.Unauthorized();
                }

                await accountService.LoginWithGoogleAsync(result.Principal);

                // Safe redirect handling
                var target = GetSafeReturnUrl(returnUrl, context, configuration);
                if (target.IsAbsoluteUri)
                {
                    // Allowed absolute URL (whitelisted frontend)
                    return Results.Redirect(target.ToString());
                }
                // Safe relative path on this host
                return Results.LocalRedirect(target.ToString());
            })
            .WithTags("Authenication")
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

        static Uri GetSafeReturnUrl(string? url, HttpContext ctx, IConfiguration config)
        {
            // 1) Allow only whitelisted absolute frontend URL
            var frontend = config["wikiquizapp:FrontendUri"];
            if (!string.IsNullOrWhiteSpace(url) && !string.IsNullOrWhiteSpace(frontend))
            {
                if (Uri.TryCreate(url, UriKind.Absolute, out var abs) && Uri.TryCreate(frontend, UriKind.Absolute, out var fe))
                {
                    if (string.Equals(abs.Scheme, fe.Scheme, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(abs.Host, fe.Host, StringComparison.OrdinalIgnoreCase)
                        && abs.Port == fe.Port)
                    {
                        // Absolute URL matches configured frontend origin
                        return abs;
                    }
                }
            }

            // 2) Otherwise allow only safe relative paths on this host
            var fallback = "/";
            if (!string.IsNullOrWhiteSpace(url) && Uri.TryCreate(url, UriKind.Relative, out var rel))
            {
                var s = url;
                if (s.StartsWith('/') && !s.StartsWith("//") && !s.StartsWith(@"/\\"))
                {
                    return new Uri(s, UriKind.Relative);
                }
            }
            return new Uri(fallback, UriKind.Relative);
        }
    }
}