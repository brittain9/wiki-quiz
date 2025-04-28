using Microsoft.EntityFrameworkCore;
using WikiQuizGenerator.Data;

var builder = WebApplication.CreateBuilder(args);

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

app.Logger.LogInformation("The web api is now running!");

app.Run();