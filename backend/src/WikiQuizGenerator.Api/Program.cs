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

    // Only use Azure App Configuration in non-development environments
    // In development, the default configuration includes appsettings.Development.json
    if (!builder.Environment.IsDevelopment())
    {
        Log.Information("Configuring Azure App Configuration in non-development environment");
        try
        {
            string endpoint = Environment.GetEnvironmentVariable("AZURE_APP_CONFIG_ENDPOINT");
            Log.Information("App Config Endpoint: {Endpoint}", endpoint ?? "NULL");
            
            if (string.IsNullOrEmpty(endpoint))
            {
                throw new InvalidOperationException("The environment variable `AZURE_APP_CONFIG_ENDPOINT` was not found.");
            }

            var managedIdentityClientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");

            Log.Information("Attempting to connect to Azure App Configuration...");
            builder.Configuration.AddAzureAppConfiguration(options =>
            {
                options.Connect(new Uri(endpoint), new DefaultAzureCredential(new DefaultAzureCredentialOptions
                {
                    ManagedIdentityClientId = managedIdentityClientId,
                    Diagnostics = { LoggedHeaderNames = { "x-ms-request-id" }, LoggedQueryParameters = { "api-version" }, IsLoggingEnabled = true }
                }))
                .Select(KeyFilter.Any, LabelFilter.Null)
                .Select(KeyFilter.Any, builder.Environment.EnvironmentName)  // Use the current environment as a label filter
                .ConfigureKeyVault(kv =>
                {
                    Log.Information("Configuring Key Vault integration for App Configuration");
                    kv.SetCredential(new DefaultAzureCredential(new DefaultAzureCredentialOptions 
                    { 
                        ManagedIdentityClientId = managedIdentityClientId 
                    }));
                });
            });
            
            Log.Information("Successfully connected to Azure App Configuration");
        }
        catch (Exception ex)
        {
            // Log the error but continue - we'll use local configuration
            Log.Error(ex, "Failed to connect to Azure App Configuration. Using local configuration instead.");
        }
    }

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