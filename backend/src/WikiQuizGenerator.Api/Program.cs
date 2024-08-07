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

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddSerilog((services, lc) => lc
        .ReadFrom.Configuration(builder.Configuration) // appsettings.json
        .ReadFrom.Services(services));

    // TODO: Make both AI services avaliable and able to switch between them
    builder.Services.AddOpenAIService(builder.Configuration);
    // builder.Services.AddPerplexityAIService(builder.Configuration);

    builder.Services.AddDataServices();

    builder.Services.AddScoped<IWikipediaContentProvider, WikipediaContentProvider>();

    builder.Services.AddSingleton<PromptManager>();
    builder.Services.AddScoped<IQuestionGenerator, QuestionGenerator>();

    builder.Services.AddScoped<IQuizGenerator, QuizGenerator>();


    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowReactApp",
            builder => builder
                .WithOrigins("http://localhost:5173") // React app's URL
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

