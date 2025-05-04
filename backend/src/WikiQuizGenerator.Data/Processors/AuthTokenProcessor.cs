using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WikiQuizGenerator.Data.Options;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Models;

namespace WikiQuizGenerator.Data.Processors;

public class AuthTokenProcessor : IAuthTokenProcessor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JwtOptions _jwtOptions;
    
    public AuthTokenProcessor(IOptions<JwtOptions> jwtOptions, IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _jwtOptions = jwtOptions.Value;
    }

    public (string jwtToken, DateTime expiresAtUtc) GenerateJwtToken(User user)
    {
        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtOptions.Secret));

        var credentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.ToString())
        };

        var expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationTimeInMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

        return (jwtToken, expires);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public void WriteAuthTokenAsHttpOnlyCookie(string cookieName, string token,
        DateTime expiration)
    {
        // Get the current HTTP context - required for cookie operations
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new InvalidOperationException("HttpContext is not available");
        }

        // Configure cookie options for better cross-domain compatibility
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,       // Prevent JavaScript access for security
            Expires = expiration,  // Set cookie expiration time
            IsEssential = true,    // Mark as essential for GDPR compliance
            
            // In development, we might not use HTTPS, but in production it should be secure
            Secure = context.Request.IsHttps, 
            
            // For cross-origin requests, SameSite=None is needed, but requires Secure=true
            // SameSite=Lax is a reasonable fallback for non-HTTPS development environments
            SameSite = context.Request.IsHttps ? SameSiteMode.None : SameSiteMode.Lax,
            
            // Ensure cookie is sent to all paths
            Path = "/",
        };

        // If the token is empty, it means we're clearing the cookie (logout)
        if (string.IsNullOrEmpty(token))
        {
            // For cookie deletion, we need to expire it in the past
            cookieOptions.Expires = DateTime.UtcNow.AddDays(-1);
        }

        // Set the cookie in the response
        context.Response.Cookies.Append(cookieName, token, cookieOptions);
    }
}