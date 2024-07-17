using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using WikiQuizGenerator.Core.Interfaces;
using WikiQuizGenerator.LLM;
using WikiQuizGenerator.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Configure Semantic Kernel
builder.Services.AddSingleton(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var openAiApiKey = configuration["OpenAI:ApiKey"];

    if(string.IsNullOrEmpty(openAiApiKey)){
        throw new InvalidOperationException("OpenAI API key not configured in appsettings.json");
    }
    
    var kernelBuilder = Kernel.CreateBuilder()
        .AddOpenAIChatCompletion("gpt-3.5-turbo", openAiApiKey);
    
    return kernelBuilder.Build();
});

// Register services
builder.Services.AddSingleton<IQuizGenerator, SemanticKernelQuizGenerator>();
builder.Services.AddSingleton<IWikipediaRepository, WikipediaRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();