using System.Text;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Identity;
using Npgsql;
using System.Threading.RateLimiting;
using WikiQuizGenerator.Core;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Models;
using WikiQuizGenerator.Core.Services;
using WikiQuizGenerator.Data;
using WikiQuizGenerator.LLM;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using WikiQuizGenerator.Data.Options;
using WikiQuizGenerator.Data.Processors;
using Serilog;

public partial class Program
{
    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        // Check if Swagger should be enabled (either in Development or explicitly via env var)
        bool enableSwagger = environment.IsDevelopment() || 
                             string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENABLESWAGGER"), "true", StringComparison.OrdinalIgnoreCase);
        
        if (enableSwagger)
        {
            services.AddOpenApi();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        }
        
        services.Configure<JwtOptions>(
            configuration.GetSection(JwtOptions.JwtOptionsKey));

        string frontendUri = configuration["wikiquizapp:FrontendUri"];
        if (string.IsNullOrWhiteSpace(frontendUri))
            throw new ArgumentNullException(nameof(frontendUri), "frontendUri is not configured.");
        
        
        services.AddCors(options =>
        {
            // Use configured frontend URI
            options.AddPolicy("AllowReactApp",
                builder => builder
                    .WithOrigins(frontendUri)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
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
                        PermitLimit = 100,        // 100 requests
                        Window = TimeSpan.FromMinutes(10) // per 10 minutes
                    }));

            options.AddPolicy("QuizLimit", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetPartitionKeyFromUser(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 10,         // 10 quiz generations
                        Window = TimeSpan.FromMinutes(10),
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

        string connectionString = configuration["wikiquizapp:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentNullException(nameof(connectionString), "ConnectionString is not configured.");
        services.AddDataServices(connectionString);
        
        services.AddScoped<IAuthTokenProcessor, AuthTokenProcessor>();
        services.AddScoped<IAccountService, AccountService>();

        // TODO: These lifetimes could use work
        services.AddScoped<IWikipediaContentProvider, WikipediaContentProvider>(); // This also probably doesn't need DE
        services.AddSingleton<PromptManager>(); // This probably doesn't need depedency injection

        string openAiApiKey = configuration["wikiquizapp:OpenAIApiKey"];
        if (string.IsNullOrWhiteSpace(openAiApiKey))
            throw new ArgumentNullException(nameof(openAiApiKey), "OpenAIAPiKey is not configured.");
        services.AddScoped<IAiServiceManager>(serviceProvider =>
            new AiServiceManager(openAiApiKey)
        );

        services.AddSingleton<IQuestionGeneratorFactory, QuestionGeneratorFactory>();
        services.AddTransient<IQuizGenerator, QuizGenerator>();

        services.AddAuthentication(opt =>
        {
            opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddCookie().AddGoogle(options =>
        {
            var clientId = configuration["wikiquizapp:AuthGoogleClientID"];

            if (clientId == null)
            {
                throw new ArgumentNullException(nameof(clientId));
            }
    
            var clientSecret = configuration["wikiquizapp:AuthGoogleClientSecret"];
    
            if (clientSecret == null)
            {
                throw new ArgumentNullException(nameof(clientSecret));
            }

            options.ClientId = clientId;
            options.ClientSecret = clientSecret;
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            
            // Log the callback path to verify it's being used
            Log.Information("Google OAuth callback path set to: {CallbackPath}", options.CallbackPath);
    
        }).AddJwtBearer(options =>
        {
            var jwtOptions = configuration.GetSection(JwtOptions.JwtOptionsKey)
                .Get<JwtOptions>() ?? throw new ArgumentException(nameof(JwtOptions));

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret))
            };

            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    context.Token = context.Request.Cookies["ACCESS_TOKEN"];
                    return Task.CompletedTask;
                }
            };
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