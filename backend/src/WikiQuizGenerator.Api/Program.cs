using Microsoft.EntityFrameworkCore;
using WikiQuizGenerator.Api.Extensions;
using WikiQuizGenerator.Data;

var builder = WebApplication.CreateBuilder(args);

// Add logging configuration
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ConfigureCustomLogging(builder.Configuration);
    // Enable all log levels during development
    if (builder.Environment.IsDevelopment())
    {
        loggingBuilder.SetMinimumLevel(LogLevel.Debug);
    }
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