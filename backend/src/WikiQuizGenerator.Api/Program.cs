using Microsoft.EntityFrameworkCore;
using Serilog;
using WikiQuizGenerator.Api;
using WikiQuizGenerator.Data;
using WikiQuizGenerator.Extensions;

// Bootstrap logger for start up
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    ConfigureServices(builder.Services, builder.Configuration);

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<WikiQuizDbContext>();
        dbContext.Database.Migrate();
    }

    app.UseSerilogRequestLogging();

    app.UseMiddleware<ErrorHandlerMiddleware>();

    app.MapQuizEndpoints();

    app.MapAiServiceEndpoints();

    app.MapSubmissionEndpoints();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors("AllowReactApp");

    Log.Information("The web api is now running!");
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

