using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.IdentityModel.Tokens;
using WikiQuizGenerator.Core;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.Data;
using WikiQuizGenerator.LLM;
using WikiQuizGenerator.Data.Options;
using WikiQuizGenerator.Data.Repositories;
using WikiQuizGenerator.Data.Processors;
using WikiQuizGenerator.Core.Services;
using WikiQuizGenerator.Core.Models;
using Microsoft.AspNetCore.Identity;


public partial class Program
{
    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenApi();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.Configure<JwtOptions>(
            configuration.GetSection(JwtOptions.JwtOptionsKey));

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

        services.AddIdentity<User, IdentityRole<Guid>>(opt =>
        {
            opt.Password.RequireDigit = true;
            opt.Password.RequireLowercase = true;
            opt.Password.RequireNonAlphanumeric = true;
            opt.Password.RequireUppercase = true;
            opt.Password.RequiredLength = 8;
            opt.User.RequireUniqueEmail = true;
        }).AddEntityFrameworkStores<WikiQuizDbContext>();

        services.AddDataServices();

        services.AddScoped<IAuthTokenProcessor, AuthTokenProcessor>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAccountService, AccountService>();

        // TODO: These lifetimes could use work
        services.AddScoped<IWikipediaContentProvider, WikipediaContentProvider>();
        services.AddSingleton<PromptManager>();
        services.AddScoped<IAiServiceManager, AiServiceManager>();

        services.AddSingleton<IQuestionGeneratorFactory, QuestionGeneratorFactory>();
        services.AddTransient<IQuizGenerator, QuizGenerator>();

        services.AddAuthentication(opt =>
        {
            opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddCookie().AddGoogle(options =>
        {
        var clientId = configuration["Authentication:Google:ClientId"];

        if (clientId == null)
        {
            throw new ArgumentNullException(nameof(clientId));
        }
        
        var clientSecret = configuration["Authentication:Google:ClientSecret"];
    
        if (clientSecret == null)
        {
            throw new ArgumentNullException(nameof(clientSecret));
        }

        options.ClientId = clientId;
        options.ClientSecret = clientSecret;
        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

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
}