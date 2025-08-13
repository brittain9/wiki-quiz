using WikiQuizGenerator.Api;
using WikiQuizGenerator.Api.Endpoints;
using WikiQuizGenerator.Middleware;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using System.Text.Json;
 

public partial class Program
{
    private static void ConfigurePipeline(WebApplication app)
    {
        app.UseForwardedHeaders();

        // Keep request logging lightweight to reduce cold-start overhead
        app.UseSerilogRequestLogging(options =>
        {
            options.GetLevel = (httpContext, elapsed, ex) => Serilog.Events.LogEventLevel.Information;
            options.EnrichDiagnosticContext = (diag, httpContext) => { };
        });

        if (app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
            app.UseHsts();
        }

        app.UseCors("AllowReactApp");

        app.UseMiddleware<ErrorHandlerMiddleware>();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseRequestTimeouts();
        app.UseRateLimiter();
        
        app.MapAuthEndpoints();
        app.MapQuizEndpoints();
        app.MapAiEndpoints();
        app.MapUserEndpoints();
        
        var liveness = (HttpContext ctx) =>
        {
            ctx.Response.Headers.CacheControl = "no-store";
            return Results.NoContent();
        };

        app.MapGet("/health/live", liveness).AllowAnonymous();
        app.MapGet("/api/health/live", liveness).AllowAnonymous();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
    }
}