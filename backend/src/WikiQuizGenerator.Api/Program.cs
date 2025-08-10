using DotNetEnv;
using DotNetEnv.Configuration;
using Serilog;
using WikiQuizGenerator.Data.Cosmos;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting up WikiQuizGenerator.Api");
    var builder = WebApplication.CreateBuilder(args);

    if (builder.Environment.IsDevelopment())
    {
        builder.Configuration.AddDotNetEnv(".env", LoadOptions.TraversePath());
    }

    // Configure Serilog from configuration
    builder.Host.UseSerilog((context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));

    Log.Information("Configuring services");
    ConfigureServices(builder.Services, builder.Configuration, builder.Environment);

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        try
        {
            Log.Information("Ensuring Cosmos DB containers exist");
            await scope.ServiceProvider
                .GetRequiredService<CosmosDbContext>()
                .EnsureContainersExistAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred during Cosmos DB initialization");
            throw;
        }
    }

    Log.Information("Configuring the pipeline");
    ConfigurePipeline(app);

    Log.Information("Running the application");
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