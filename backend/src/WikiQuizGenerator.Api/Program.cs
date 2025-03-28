using Microsoft.EntityFrameworkCore;
using WikiQuizGenerator.Api;
using WikiQuizGenerator.Data;
using WikiQuizGenerator.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Configure services
ConfigureServices(builder.Services, builder.Configuration);

// Add logging configuration
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConfiguration(builder.Configuration.GetSection("Logging"));
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();
});

var app = builder.Build();

// Database migration
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<WikiQuizDbContext>();
    dbContext.Database.Migrate();
}

// Middleware pipeline
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ErrorHandlerMiddleware>();

// Endpoints
app.MapQuizEndpoints();
app.MapAiServiceEndpoints();
app.MapSubmissionEndpoints();

// Development configuration
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReactApp");

app.Logger.LogInformation("The web api is now running!");

app.Run();