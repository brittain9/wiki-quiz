using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WikiQuizGenerator.Core.Options;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.DomainObjects;

namespace WikiQuizGenerator.Core.Services;

public class AuthTokenService : IAuthTokenService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JwtOptions _jwtOptions;
    
    public AuthTokenService(IOptions<JwtOptions> jwtOptions, IHttpContextAccessor httpContextAccessor)
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
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
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
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            throw new InvalidOperationException("HttpContext is not available");
        }

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = expiration,
            IsEssential = true,
            Secure = false,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Domain = null
        };

        if (string.IsNullOrEmpty(token))
        {
            cookieOptions.Expires = DateTime.UtcNow.AddDays(-1);
        }

        context.Response.Cookies.Append(cookieName, token, cookieOptions);
    }
}


