using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using WikiQuizGenerator.Data;

var builder = WebApplication.CreateBuilder(args);

string endpoint = builder.Configuration.GetValue<string>("Endpoints:AppConfiguration")
    ?? throw new InvalidOperationException("The setting `Endpoints:AppConfiguration` was not found.");


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

// Configure services (calls your partial method)
ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

// Database migration
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<WikiQuizDbContext>();
    dbContext.Database.Migrate();

    // Seed ModelConfig data
    var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "aiservices.json");
    await dbContext.SeedModelConfigsAsync(configPath);
}

ConfigurePipeline(app);

app.Run();