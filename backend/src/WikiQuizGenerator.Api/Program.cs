using System.Net;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.LLM;
using WikiQuizGenerator.Data;
using WikiQuizGenerator.Core;
using WikiQuizGenerator.Api;
using Serilog;

// Bootstrap logger for start up, config not av
 Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddSerilog((services, lc) => lc
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            path: "logs/log-.txt",
            rollingInterval: RollingInterval.Day));

    // TODO: Make both AI services avaliable and able to switch between them
    // Choose AI service here
    // builder.Services.AddOpenAIService(builder.Configuration);
    builder.Services.AddPerplexityAIService(builder.Configuration);

    builder.Services.AddSingleton<IQuestionGenerator, QuestionGenerator>();
    builder.Services.AddSingleton<IQuizGenerator, QuizGenerator>();
    builder.Services.AddDataServices();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowReactApp",
            builder => builder
                .WithOrigins("http://localhost:3000") // React app's URL
                .AllowAnyMethod()
                .AllowAnyHeader());
    });

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    app.MapQuizEndpoints();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseCors("AllowReactApp");

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

