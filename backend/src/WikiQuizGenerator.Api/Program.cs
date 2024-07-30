using System.Net;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.LLM;
using WikiQuizGenerator.Data;
using WikiQuizGenerator.Core;
using WikiQuizGenerator.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// TOOD: Make both AI services avaliable and able to switch between them
// Choose AI service here
builder.Services.AddOpenAIService(builder.Configuration);
//builder.Services.AddPerplexityAIService(builder.Configuration);

builder.Services.AddSingleton<IQuestionGenerator, QuestionGenerator>();
builder.Services.AddSingleton<IQuizGenerator, QuizGenerator>();

builder.Services.AddDataServices(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder => builder
            .WithOrigins("http://localhost:3000") // React app's URL
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

app.MapQuizEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowReactApp");

app.Run();