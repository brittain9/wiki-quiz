using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using WikiQuizGenerator.Core;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Data;
using WikiQuizGenerator.LLM;

public partial class Program
{
    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddDataServices();

        // TODO: These lifetimes could use work
        services.AddScoped<IWikipediaContentProvider, WikipediaContentProvider>();
        services.AddSingleton<PromptManager>();
        services.AddScoped<IAiServiceManager, AiServiceManager>();

        services.AddSingleton<IQuestionGeneratorFactory, QuestionGeneratorFactory>();
        services.AddTransient<IQuizGenerator, QuizGenerator>();

        // Add Authentication Services
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            options.Cookie.SameSite = SameSiteMode.Lax;
        })
        .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
        {
            options.ClientSecret = configuration["Authentication:Google:ClientSecret"] ?? throw new InvalidOperationException("Google ClientSecret missing");
            options.ClientId = configuration["Authentication:Google:ClientId"] ?? throw new InvalidOperationException("Google ClientId missing");
            options.CallbackPath = "/api/auth/signin-google";
            options.Scope.Add("profile");
            options.Scope.Add("email");
        })
        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                    configuration["JwtSettings:Key"] ?? throw new InvalidOperationException("JWT Key missing"))),

                ValidateIssuer = true,
                ValidIssuer = configuration["JwtSettings:Issuer"] ?? throw new InvalidOperationException("JWT Issuer missing"),

                ValidateAudience = true,
                ValidAudience = configuration["JwtSettings:Audience"] ?? throw new InvalidOperationException("JWT Audience missing"),

                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();

        services.AddCors(options =>
        {
            options.AddPolicy("AllowReactApp",
                builder => builder
                    .WithOrigins("http://localhost:5173")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
        });
    }
}