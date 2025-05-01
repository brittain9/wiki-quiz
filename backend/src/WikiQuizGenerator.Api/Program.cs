using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using WikiQuizGenerator.Data;

var builder = WebApplication.CreateBuilder(args);

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

using (var scope = app.Services.CreateScope())
{
    // Database migration
    var dbContext = scope.ServiceProvider.GetRequiredService<WikiQuizDbContext>();
    dbContext.Database.Migrate();

    // Seed ModelConfig data
    var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "aiservices.json");
    await dbContext.SeedModelConfigsAsync(configPath);
}

ConfigurePipeline(app);

app.Run();