using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Serilog;
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

    Log.Information("Configuring services");
    ConfigureServices(builder.Services, builder.Configuration, builder.Environment);

    var app = builder.Build();


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
    Log.Information("Configuring the pipeline");
    ConfigurePipeline(app);
    app

    Log.Information("Running the application");
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