using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using WikiQuizGenerator.Core;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Core.Options;
using WikiQuizGenerator.Core.Services;
using WikiQuizGenerator.Data.Cosmos;
using WikiQuizGenerator.LLM;
public partial class Program
{
    /// <summary>
    /// Configures all application services for dependency injection
    /// </summary>
    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        // Core configuration
        ConfigureOptions(services, configuration);
        
        // Development tools
        ConfigureDevelopmentServices(services, environment);
        
        // Monitoring and telemetry
        ConfigureTelemetry(services, configuration);
        
        // Security and middleware
        ConfigureCors(services, configuration);
        ConfigureRateLimiting(services);
        ConfigureTimeouts(services);
        
        // Data and business services
        ConfigureDataServices(services, configuration);
        ConfigureBusinessServices(services, configuration);
        
        // Authentication and authorization
        ConfigureAuthentication(services, configuration);
        
        // Health checks
        ConfigureHealthChecks(services, configuration);
    }
    
    /// <summary>
    /// Configures application options and settings
    /// </summary>
    private static void ConfigureOptions(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.JwtOptionsKey));
    }
    
    /// <summary>
    /// Configures development-only services (Swagger, etc.)
    /// </summary>
    private static void ConfigureDevelopmentServices(IServiceCollection services, IHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            services.AddOpenApi();
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
        }
    }
    
    /// <summary>
    /// Configures telemetry and monitoring services
    /// </summary>
    private static void ConfigureTelemetry(IServiceCollection services, IConfiguration configuration)
    {
        // Enable Application Insights only when connection string is provided
        var appInsightsConn = configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
        if (!string.IsNullOrWhiteSpace(appInsightsConn))
        {
            services.AddApplicationInsightsTelemetry(options =>
            {
                options.ConnectionString = appInsightsConn;
            });
        }
    }
    
    /// <summary>
    /// Configures CORS policies for frontend communication
    /// </summary>
    private static void ConfigureCors(IServiceCollection services, IConfiguration configuration)
    {
        var frontendUri = configuration["wikiquizapp:FrontendUri"] 
            ?? throw new ArgumentNullException("frontendUri", "Frontend URI is not configured.");

        services.AddCors(options =>
        {
            options.AddPolicy("AllowReactApp", builder => builder
                .WithOrigins(frontendUri)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
        });
    }
    
    /// <summary>
    /// Configures rate limiting to prevent abuse
    /// </summary>
    private static void ConfigureRateLimiting(IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Global rate limiter: 100 requests per 10 minutes per user/IP
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetPartitionKeyFromUser(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(10)
                    }));

            // Quiz-specific rate limiter: 10 quiz generations per 10 minutes
            options.AddPolicy("QuizLimit", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: GetPartitionKeyFromUser(httpContext),
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(10),
                        QueueLimit = 0
                    }));

            // Configure standardized rate limit response
            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/json";

                var retryAfterSeconds = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                    ? (int)retryAfter.TotalSeconds
                    : 60;

                context.HttpContext.Response.Headers.Append("Retry-After", retryAfterSeconds.ToString());

                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    title = "Too Many Requests",
                    detail = $"Rate limit exceeded. Please try again after {retryAfterSeconds} seconds.",
                    retryAfter = retryAfterSeconds
                }, token);
            };
        });
    }
    
    /// <summary>
    /// Configures request timeout policies
    /// </summary>
    private static void ConfigureTimeouts(IServiceCollection services)
    {
        services.AddRequestTimeouts(options =>
        {
            options.DefaultPolicy = new RequestTimeoutPolicy
            {
                Timeout = TimeSpan.FromSeconds(60),
                TimeoutStatusCode = 504
            };
        });
    }
    
    /// <summary>
    /// Configures data access services (Cosmos DB, repositories)
    /// </summary>
    private static void ConfigureDataServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddCosmosDataServices(configuration);
        services.AddHttpContextAccessor();
    }
    
    /// <summary>
    /// Configures business logic services
    /// </summary>
    private static void ConfigureBusinessServices(IServiceCollection services, IConfiguration configuration)
    {
        // Authentication and user services
        services.AddScoped<IAuthTokenService, AuthTokenService>();
        services.AddSingleton<IPointsService, PointsService>();

        // Content and data services
        services.AddHttpClient<IWikipediaContentService, WikipediaContentService>();
        services.AddSingleton<IModelConfigService, ModelConfigService>();
        
        // AI and quiz generation services
        services.AddSingleton<PromptManager>();
        
        var openAiApiKey = configuration["wikiquizapp:OpenAIApiKey"] 
            ?? throw new ArgumentNullException("OpenAIApiKey", "OpenAI API key is not configured.");
            
        services.AddScoped<IAiServiceManager>(_ => new AiServiceManager(openAiApiKey));
        services.AddSingleton<IQuestionGeneratorFactory, QuestionGeneratorFactory>();
        services.AddScoped<IQuizGenerator, QuizGenerator>();
    }
    
    /// <summary>
    /// Configures authentication and authorization services
    /// </summary>
    private static void ConfigureAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddCookie()
        .AddGoogle(options =>
        {
            var clientId = configuration["wikiquizapp:AuthGoogleClientID"] 
                ?? throw new ArgumentNullException("AuthGoogleClientID", "Google OAuth client ID is not configured.");
                
            var clientSecret = configuration["wikiquizapp:AuthGoogleClientSecret"] 
                ?? throw new ArgumentNullException("AuthGoogleClientSecret", "Google OAuth client secret is not configured.");

            options.ClientId = clientId;
            options.ClientSecret = clientSecret;
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            
            Log.Information("Google OAuth callback path set to: {CallbackPath}", options.CallbackPath);
        })
        .AddJwtBearer(options =>
        {
            var jwtOptions = configuration.GetSection(JwtOptions.JwtOptionsKey).Get<JwtOptions>() 
                ?? throw new ArgumentException("JWT options are not properly configured.", nameof(JwtOptions));

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

            // Extract JWT from cookies for browser-based requests
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
    }
    
    /// <summary>
    /// Configures health check services for monitoring application status
    /// </summary>
    private static void ConfigureHealthChecks(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy("API is running"), 
                tags: new[] { "live" })
            .AddCheck("cosmosdb", () => 
            {
                try
                {
                    var cosmosOptions = configuration.GetSection("CosmosDb")
                        .Get<WikiQuizGenerator.Data.Cosmos.Configuration.CosmosDbOptions>();
                    
                    return cosmosOptions?.ConnectionString == null
                        ? HealthCheckResult.Unhealthy("Cosmos DB connection string is not configured")
                        : HealthCheckResult.Healthy("Cosmos DB configuration is valid");
                }
                catch (Exception ex)
                {
                    return HealthCheckResult.Unhealthy("Cosmos DB check failed", ex);
                }
            }, tags: new[] { "ready", "database", "cosmosdb" })
            .AddCheck("openai-config", () => 
            {
                var openAiKey = configuration["wikiquizapp:OpenAIApiKey"];
                return string.IsNullOrWhiteSpace(openAiKey)
                    ? HealthCheckResult.Unhealthy("OpenAI API key is not configured")
                    : HealthCheckResult.Healthy("OpenAI configuration is valid");
            }, tags: new[] { "ready", "configuration", "external" })
            .AddCheck("jwt-config", () => 
            {
                var jwtOptions = configuration.GetSection(JwtOptions.JwtOptionsKey).Get<JwtOptions>();
                return jwtOptions == null || string.IsNullOrWhiteSpace(jwtOptions.Secret)
                    ? HealthCheckResult.Unhealthy("JWT configuration is invalid")
                    : HealthCheckResult.Healthy("JWT configuration is valid");
            }, tags: new[] { "ready", "configuration" });
    }

    /// <summary>
    /// Generates a partition key for rate limiting based on user authentication status
    /// Uses user ID for authenticated users, IP + browser fingerprint for anonymous users
    /// </summary>
    private static string GetPartitionKeyFromUser(HttpContext httpContext)
    {
        // Authenticated users: use user ID for accurate rate limiting
        if (httpContext.User?.Identity?.IsAuthenticated == true)
        {
            var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
                return $"u:{userId}";
        }

        // Anonymous users: combine IP address with basic browser fingerprinting
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
        var userAgent = httpContext.Request.Headers.UserAgent.ToString();
        
        if (!string.IsNullOrEmpty(userAgent))
        {
            // Use first segment of user agent for stability across minor version changes
            var segments = userAgent.Split(' ');
            var browserInfo = segments.Length > 0 ? segments[0] : "";
            var browserHash = browserInfo.GetHashCode() & 0x7FFFFFFF; // Ensure positive hash
            
            return $"i:{ipAddress}:{browserHash}";
        }

        return $"i:{ipAddress}";
    }
}
