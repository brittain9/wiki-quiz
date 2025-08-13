using WikiQuizGenerator.Api;
using WikiQuizGenerator.Api.Endpoints;
using WikiQuizGenerator.Middleware;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

public partial class Program
{
    private static void ConfigurePipeline(WebApplication app)
    {
        app.UseSerilogRequestLogging();

        // In non-development environments (e.g., Azure), honor proxy headers for correct scheme/host
        if (!app.Environment.IsDevelopment())
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost,
                KnownNetworks = { },
                KnownProxies = { }
            });
        }

        if (!string.Equals(Environment.GetEnvironmentVariable("SKIP_APP_CONFIG"), "true", StringComparison.OrdinalIgnoreCase))
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
        
        // Add health check endpoints
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";
                
                var result = new
                {
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(x => new
                    {
                        name = x.Key,
                        status = x.Value.Status.ToString(),
                        exception = x.Value.Exception?.Message,
                        duration = x.Value.Duration.ToString()
                    })
                };
                
                await context.Response.WriteAsync(JsonSerializer.Serialize(result));
            }
        });
        
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });
        
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live") || check.Name == "self"
        });

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