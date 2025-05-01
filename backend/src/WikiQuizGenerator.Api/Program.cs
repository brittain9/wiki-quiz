using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Serilog;
using Serilog.Events;
using WikiQuizGenerator.Data;

// Configure Serilog first, before any other code runs
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting up WikiQuizGenerator.Api");
    
    var builder = WebApplication.CreateBuilder(args);

    // Configure Serilog from configuration
    builder.Host.UseSerilog((context, services, configuration) => 
        configuration.ReadFrom.Configuration(context.Configuration));

    // Only use Azure App Configuration in non-development environments
    // In development, the default configuration includes appsettings.Development.json
    if (!builder.Environment.IsDevelopment())
    {
        string endpoint = Environment.GetEnvironmentVariable("AZURE_APP_CONFIG_ENDPOINT")
            ?? throw new InvalidOperationException("The environment variable `AZURE_APP_CONFIG_ENDPOINT` was not found.");

        builder.Configuration.AddAzureAppConfiguration(options =>
        {
            options.Connect(new Uri(endpoint), new DefaultAzureCredential())
                .Select(KeyFilter.Any, LabelFilter.Null)
                .Select(KeyFilter.Any, builder.Environment.EnvironmentName.ToUpper()) // I configured in azure all uppercase
                .ConfigureKeyVault(kv =>
                {
                    kv.SetCredential(new DefaultAzureCredential());
                });
        });
    }

    ConfigureServices(builder.Services, builder.Configuration, builder.Environment);

    var app = builder.Build();

    // This is now handled entirely by the Serilog configuration
    app.UseSerilogRequestLogging();

    using (var scope = app.Services.CreateScope())
    {
        try
        {
            // Database migration
            Log.Information("Running database migrations");
            var dbContext = scope.ServiceProvider.GetRequiredService<WikiQuizDbContext>();
            dbContext.Database.Migrate();

            // Seed ModelConfig data
            Log.Information("Seeding model configurations");
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "aiservices.json");
            await dbContext.SeedModelConfigsAsync(configPath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred during startup");
            throw;
        }
    }

    ConfigurePipeline(app);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}