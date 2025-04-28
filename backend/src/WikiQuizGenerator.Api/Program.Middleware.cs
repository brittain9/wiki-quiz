using WikiQuizGenerator.Api;
using WikiQuizGenerator.Api.Endpoints;
using WikiQuizGenerator.Middleware;
using Microsoft.AspNetCore.HttpOverrides;

public partial class Program
{
    private static void ConfigurePipeline(WebApplication app)
    {
        // Configure forwarded headers if behind a proxy
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        app.UseCors("AllowReactApp");

        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseMiddleware<ErrorHandlerMiddleware>();

        app.UseRequestTimeouts();
        app.UseRateLimiter();

        // Only use HTTPS redirection in Production
        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
            app.UseHsts();
        }

        // Use cookie policy before authentication
        app.UseCookiePolicy();

        app.UseRouting();

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
    }
}