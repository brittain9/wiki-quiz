﻿using WikiQuizGenerator.Api;
using WikiQuizGenerator.Api.Endpoints;
using WikiQuizGenerator.Middleware;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;

public partial class Program
{
    private static void ConfigurePipeline(WebApplication app)
    {
        app.UseSerilogRequestLogging();

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            // Trust the X-Forwarded-Proto header to determine the request scheme (http/https)
            // Trust X-Forwarded-For to determine client IP
            // Trust X-Forwarded-Host to determine the original host requested by the client
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost,
            KnownNetworks = { },
            KnownProxies = { }
        });

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