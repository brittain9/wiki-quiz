using Microsoft.Extensions.Logging.Console;

namespace WikiQuizGenerator.Api.Extensions;

public static class LoggingExtensions
{
    public static ILoggingBuilder ConfigureCustomLogging(this ILoggingBuilder builder, IConfiguration configuration)
    {
        // Add configuration from appsettings.json
        builder.AddConfiguration(configuration.GetSection("Logging"));
        
        // Always add console logger
        builder.AddSimpleConsole(options =>
        {
            options.SingleLine = true;
            options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
            options.UseUtcTimestamp = true;
            options.ColorBehavior = LoggerColorBehavior.Enabled;
            options.IncludeScopes = false;
        });
        
        // Add debug output for development
        builder.AddDebug();
        
        return builder;
    }
} 