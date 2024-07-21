using System.Net;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.LLM;
using WikiQuizGenerator.Data;
using WikiQuizGenerator.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenAIService(builder.Configuration);
builder.Services.AddSingleton<IQuestionGenerator, SemanticKernelQuestionGenerator>();
builder.Services.AddSingleton<IQuizGenerator, QuizGenerator>();
builder.Services.AddDataServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Dev cert is annoying to set up on Ubuntu and adds more requirements for now
// app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();