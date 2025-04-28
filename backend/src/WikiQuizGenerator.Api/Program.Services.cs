using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.CookiePolicy;
using WikiQuizGenerator.Core;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Data;
using WikiQuizGenerator.LLM;
using WikiQuizGenerator.Data.Options;
using WikiQuizGenerator.Data.Repositories;
using WikiQuizGenerator.Core.Services;
using WikiQuizGenerator.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http.Timeouts;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;


public partial class Program
{
    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenApi();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddCors(options =>
        {
            options.AddPolicy("AllowReactApp",
                builder => builder
                    .WithOrigins("http://localhost:5173")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .SetIsOriginAllowed(_ => true));
        });

        // Configure Identity
        services.AddIdentity<User, IdentityRole<Guid>>(opt =>
        {
            opt.Password.RequireDigit = true;
            opt.Password.RequireLowercase = true;
            opt.Password.RequireNonAlphanumeric = true;
            opt.Password.RequireUppercase = true;
            opt.Password.RequiredLength = 8;
            opt.User.RequireUniqueEmail = true;
        }).AddEntityFrameworkStores<WikiQuizDbContext>();

        // Configure Identity cookie settings
        services.ConfigureApplicationCookie(options =>
        {
            options.ExpireTimeSpan = TimeSpan.FromDays(14);
            options.SlidingExpiration = true;
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Use Always in production
            options.LoginPath = "/login";
            options.LogoutPath = "/logout";
            options.AccessDeniedPath = "/access-denied";
        });

        // Add rate limiting services
        services.AddRateLimiter(options =>
        {
            // Global rate limiter for all endpoints
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetPartitionKeyFromUser(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 50,        // 100 requests
                        Window = TimeSpan.FromMinutes(2) // per 2 minutes
                    }));

            options.AddPolicy("QuizLimit", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetPartitionKeyFromUser(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 2,         // 2 quiz generations
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            // Configure rate limiting response - simplified for better performance
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";
                
                var retryAfterSeconds = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter) 
                    ? (int)retryAfter.TotalSeconds 
                    : 60; // Default to 60 seconds if not specified
                
                context.HttpContext.Response.Headers.Append("Retry-After", retryAfterSeconds.ToString());
                
                await context.HttpContext.Response.WriteAsJsonAsync(new 
                {
                    title = "Too Many Requests",
                    detail = $"Rate limit exceeded. Please try again after {retryAfterSeconds} seconds.",
                    retryAfter = retryAfterSeconds
                }, token);
            };
        });

        services.AddRequestTimeouts(options =>
        {
            // Configure the default timeout policy
            options.DefaultPolicy = new RequestTimeoutPolicy
            {
                Timeout = TimeSpan.FromSeconds(60),
                TimeoutStatusCode = 504
            };
        });

        services.AddDataServices();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAccountService, AccountService>();

        // TODO: These lifetimes could use work
        services.AddScoped<IWikipediaContentProvider, WikipediaContentProvider>();
        services.AddSingleton<PromptManager>();
        services.AddScoped<IAiServiceManager, AiServiceManager>();

        services.AddSingleton<IQuestionGeneratorFactory, QuestionGeneratorFactory>();
        services.AddTransient<IQuizGenerator, QuizGenerator>();

        // Configure Authentication with cookie scheme
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
            options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        })
        // Add Google Authentication
        .AddGoogle(options =>
        {
            var clientId = configuration["Authentication:Google:ClientId"];
            var clientSecret = configuration["Authentication:Google:ClientSecret"];

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                throw new ArgumentException("Google OAuth credentials are missing");
            }

            options.ClientId = clientId;
            options.ClientSecret = clientSecret;
            options.SignInScheme = IdentityConstants.ExternalScheme;
        });

        // Configure cookie policy
        services.Configure<CookiePolicyOptions>(options =>
        {
            options.MinimumSameSitePolicy = SameSiteMode.Lax;
            options.HttpOnly = HttpOnlyPolicy.Always;
            options.Secure = CookieSecurePolicy.SameAsRequest; // Use Always in production
        });

        services.AddAuthorization();
        services.AddHttpContextAccessor();
    }

    private static string GetPartitionKeyFromUser(HttpContext httpContext)
    {
        // If user is authenticated, use user ID as the partition key (most reliable)
        if (httpContext.User?.Identity?.IsAuthenticated == true)
        {
            var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                return $"u:{userId}";
            }
        }

        // For anonymous users, combine IP and basic browser fingerprinting
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        
        // Get basic browser fingerprint from User-Agent (first 2 segments only for stability)
        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        if (!string.IsNullOrEmpty(userAgent))
        {
            // Parse just the browser name/version for better stability
            var segments = userAgent.Split(' ');
            var browserInfo = segments.Length > 0 ? segments[0] : "";
            
            return $"i:{ipAddress}:{browserInfo.GetHashCode() & 0x7FFFFFFF}"; // Positive hash only
        }
        
        return $"i:{ipAddress}";
    }
}