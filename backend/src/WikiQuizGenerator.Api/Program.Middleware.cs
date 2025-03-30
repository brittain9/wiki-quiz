using WikiQuizGenerator.Api;
using WikiQuizGenerator.Api.Endpoints;
using WikiQuizGenerator.Middleware;

public partial class Program
{
    private static void ConfigurePipeline(WebApplication app)
    {
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseMiddleware<ErrorHandlerMiddleware>();

        // app.UseHttpsRedirection();

        app.UseRouting();

        app.UseCors("AllowReactApp"); // Use CORS policy (BEFORE Authentication/Authorization)

        app.UseAuthentication();
        app.UseAuthorization();

        // Map Endpoints AFTER Auth
        app.MapAuthEndpoints();
        app.MapQuizEndpoints();
        app.MapAiServiceEndpoints();
        app.MapSubmissionEndpoints();

        // Development configuration
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        else
        {
            // Optional: Add HSTS for production later
            // app.UseHsts();
        }
    }
}