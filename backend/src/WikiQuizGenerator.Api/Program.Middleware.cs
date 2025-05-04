using WikiQuizGenerator.Api;
using WikiQuizGenerator.Api.Endpoints;
using WikiQuizGenerator.Middleware;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;

public partial class Program
{
    private static void ConfigurePipeline(WebApplication app)
    {
        app.UseSerilogRequestLogging();

        app.UseCors("AllowReactApp");

        app.UseMiddleware<ErrorHandlerMiddleware>();

        // Only use HTTPS redirection in Production
        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
            app.UseHsts();
        }
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseRequestTimeouts();
        app.UseRateLimiter();
        

        app.MapAuthEndpoints();
        app.MapQuizEndpoints();
        app.MapAiServiceEndpoints();
        app.MapSubmissionEndpoints();

        // Check if Swagger should be enabled (either in Development or explicitly via env var)
        bool enableSwagger = app.Environment.IsDevelopment() || 
                             string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENABLESWAGGER"), "true", StringComparison.OrdinalIgnoreCase);
        
        if (enableSwagger)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
    }
}