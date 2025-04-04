using Microsoft.EntityFrameworkCore;
using WikiQuizGenerator.Data;

var builder = WebApplication.CreateBuilder(args);

// Add logging configuration
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConfiguration(builder.Configuration.GetSection("Logging"));
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();
});

// Configure services (calls your partial method)
ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();

// Database migration
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<WikiQuizDbContext>();
    dbContext.Database.Migrate();
}

ConfigurePipeline(app);

app.Logger.LogInformation("The web api is now running!");

app.Run();