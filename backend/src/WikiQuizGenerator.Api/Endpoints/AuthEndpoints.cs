using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore; // For EF Core async methods
using Microsoft.IdentityModel.Tokens;
using WikiQuizGenerator.Core.Models;
using WikiQuizGenerator.Data; // Your DbContext namespace

namespace WikiQuizGenerator.Api.Endpoints;

public static class AuthEndpoints
{
    // Extension method on IEndpointRouteBuilder (WebApplication implements this)
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
                       .WithTags("Authentication");

        // Endpoint to trigger Google login challenge
        group.MapGet("/google-login", (HttpContext httpContext) =>
        {
            var properties = new AuthenticationProperties
            {
                // Relative path to the callback endpoint defined below
                RedirectUri = "signin-google"
            };
            return Results.Challenge(properties, [GoogleDefaults.AuthenticationScheme]);
        })
        .WithName("GoogleLogin");

        // Endpoint Google redirects back to (matches CallbackPath)
        // NOTE: This endpoint route is NOT under /api/auth, it's defined by GoogleDefaults
        // We map it outside the group or ensure the CallbackPath matches if grouped.
        // Mapping it at the root level is usually safest for external callbacks.
        group.MapGet("/signin-google", async (
            HttpContext httpContext,
            IConfiguration configuration,
            WikiQuizDbContext dbContext) => // Inject dependencies
        {
            var authenticateResult = await httpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded || authenticateResult.Principal == null)
            {
                var frontendUrl = configuration["JwtSettings:Audience"] ?? "/";
                return Results.Redirect($"{frontendUrl}?error=google_auth_failed");
            }

            var googlePrincipal = authenticateResult.Principal;
            var googleUserId = googlePrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = googlePrincipal.FindFirstValue(ClaimTypes.Email);
            var givenName = googlePrincipal.FindFirstValue(ClaimTypes.GivenName);
            var surname = googlePrincipal.FindFirstValue(ClaimTypes.Surname);
            var name = googlePrincipal.FindFirstValue(ClaimTypes.Name) ?? $"{givenName} {surname}".Trim();

            if (string.IsNullOrWhiteSpace(googleUserId) || string.IsNullOrWhiteSpace(email))
            {
                var frontendUrl = configuration["JwtSettings:Audience"] ?? "/";
                return Results.Redirect($"{frontendUrl}?error=google_claims_missing");
            }

            // --- Find or Create User in Your Database ---
            // !! IMPORTANT: Replace this with your actual User entity and logic !!
            var user = await dbContext.Users.FirstOrDefaultAsync(u => u.GoogleId == googleUserId);
            string appUserId;
            if (user == null)
            {
                var existingUserWithEmail = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (existingUserWithEmail != null)
                {
                    var frontendUrl = configuration["JwtSettings:Audience"] ?? "/";
                    return Results.Redirect($"{frontendUrl}?error=email_exists");
                }
                else
                {
                    var newUser = new User { GoogleId = googleUserId, Email = email, DisplayName = name, CreatedAt = DateTime.UtcNow };
                    dbContext.Users.Add(newUser);
                    await dbContext.SaveChangesAsync();
                    user = newUser;
                }
            }
            else
            {
                bool updated = false;
                if (user.DisplayName != name) { user.DisplayName = name; updated = true; }
                if (user.Email != email) { user.Email = email; updated = true; }
                if (updated) { await dbContext.SaveChangesAsync(); }
            }
            appUserId = user.Id.ToString(); // Replace with your actual ID property
            // -------------------------------------------------

            var jwtToken = GenerateJwtToken(configuration, appUserId, email, name); // Call helper

            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            var frontendCallbackUrl = configuration["FrontendSettings:CallbackUrl"] ?? throw new InvalidOperationException("Frontend CallbackUrl missing");
            return Results.Redirect($"{frontendCallbackUrl}#token={jwtToken}");
        })
        .WithName("GoogleCallback"); // Optional name for the callback route itself

        // Example protected endpoint within the /api/auth group
        group.MapGet("/me", (HttpContext httpContext) =>
        {
            if (httpContext.User.Identity?.IsAuthenticated != true)
            {
                return Results.Unauthorized();
            }
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = httpContext.User.FindFirstValue(ClaimTypes.Email);
            var name = httpContext.User.FindFirstValue(ClaimTypes.Name);
            return Results.Ok(new { Id = userId, Email = email, Name = name });
        })
        .RequireAuthorization() // Protect this endpoint
        .WithName("GetMe");
    }

    // --- Private helper function to Generate JWT ---
    private static string GenerateJwtToken(IConfiguration config, string userId, string? email, string? name)
    {
        var jwtSettings = config.GetSection("JwtSettings");
        var keyBytes = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key missing"));
        var issuer = jwtSettings["Issuer"] ?? throw new InvalidOperationException("JWT Issuer missing");
        var audience = jwtSettings["Audience"] ?? throw new InvalidOperationException("JWT Audience missing");

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
        };
        if (!string.IsNullOrEmpty(email)) claims.Add(new Claim(JwtRegisteredClaimNames.Email, email));
        if (!string.IsNullOrEmpty(name)) claims.Add(new Claim(JwtRegisteredClaimNames.Name, name));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1), // Make expiration configurable
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}