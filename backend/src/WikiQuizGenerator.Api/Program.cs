using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.LLM;
using WikiQuizGenerator.Data;
using WikiQuizGenerator.Core;
using WikiQuizGenerator.Api;
using Serilog;

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

    app.UseSerilogRequestLogging();

    app.MapQuizEndpoints();
    app.MapAiServiceEndpoints();

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

