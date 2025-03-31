using System;
using System.Collections.Generic; // For List<Claim>
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Models;
using WikiQuizGenerator.Data.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;

namespace WikiQuizGenerator.Data.Processors;

public class AuthTokenProcessor : IAuthTokenProcessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JwtOptions _jwtOptions;
    private readonly CookieOptions _baseCookieOptions;
    private readonly ILogger<AuthTokenProcessor> _logger;
    private readonly IHostEnvironment _environment;

    public AuthTokenProcessor(
        IOptions<JwtOptions> jwtOptions,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuthTokenProcessor> logger,
        IHostEnvironment environment)
    {
        _httpContextAccessor = httpContextAccessor;
        _jwtOptions = jwtOptions.Value;
        _logger = logger;
        _environment = environment;

        _baseCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            // Only use Secure=true if not in development
            Secure = !_environment.IsDevelopment(),
            // Use Lax for development, None for production with Secure=true
            SameSite = _environment.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
            Path = "/"
            // Domain = "yourdomain.com" // Set only if necessary
        };
    }

    // --- GenerateJwtToken method (no changes needed here) ---
    public (string jwtToken, DateTime expiresAtUtc) GenerateJwtToken(User user)
    {
        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtOptions.Secret));

        var credentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            // Add other claims like name, roles if needed
        };

        var expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationTimeInMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow.AddSeconds(-30),
            expires: expires,
            signingCredentials: credentials);

        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

        return (jwtToken, expires);
    }


    // --- GenerateRefreshToken method (no changes needed here) ---
     public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }


    // --- WriteAuthTokenAsHttpOnlyCookie method ---
    public void WriteAuthTokenAsHttpOnlyCookie(string cookieName, string token, DateTime expiration)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            _logger?.LogError("HttpContext is not available. Cannot write cookie '{CookieName}'.", cookieName);
            throw new InvalidOperationException("HttpContext is not available.");
        }

        // FIX: Manually create new options and copy base properties
        var options = new CookieOptions
        {
            HttpOnly = _baseCookieOptions.HttpOnly,
            Secure = _baseCookieOptions.Secure,
            SameSite = _baseCookieOptions.SameSite,
            Path = _baseCookieOptions.Path,
            Domain = _baseCookieOptions.Domain, // Copy Domain if set in base options
            // Set specific properties for writing
            Expires = expiration,
            IsEssential = true // Keep if needed for consent mechanisms
        };

        _logger?.LogDebug("Setting cookie {CookieName} with options: HttpOnly={HttpOnly}, Secure={Secure}, SameSite={SameSite}, Path={Path}",
            cookieName, options.HttpOnly, options.Secure, options.SameSite, options.Path);

        httpContext.Response.Cookies.Append(cookieName, token, options);
    }

    // --- DeleteAuthTokenCookie method ---
    public void DeleteAuthTokenCookie(string cookieName)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
             _logger?.LogWarning("HttpContext not available. Cannot delete cookie '{CookieName}'.", cookieName);
            return;
        }

        // FIX: Manually create new options and copy base properties
        var options = new CookieOptions
        {
            HttpOnly = _baseCookieOptions.HttpOnly,
            Secure = _baseCookieOptions.Secure,
            SameSite = _baseCookieOptions.SameSite,
            Path = _baseCookieOptions.Path,
            Domain = _baseCookieOptions.Domain, // Copy Domain if set in base options
            // Set specific properties for deletion
            Expires = DateTime.UtcNow.AddYears(-1) // Set expiration to the past
        };

        // Append with empty value and past expiry to delete
        httpContext.Response.Cookies.Append(cookieName, "", options);

        // Alternatively, Cookies.Delete(cookieName, options) should also work if options match exactly
        // httpContext.Response.Cookies.Delete(cookieName, options);
    }
}