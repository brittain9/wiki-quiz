using System.Net;
using System.Net.Mime;
using System.Text.Json;
using WikiQuizGenerator.Core.Models;
using WikiQuizGenerator.Core.Exceptions;

namespace WikiQuizGenerator.Middleware;

public class ErrorHandlerMiddleware
{
    private readonly ILogger<ErrorHandlerMiddleware> _logger;
    private readonly RequestDelegate _next;

    public ErrorHandlerMiddleware(ILogger<ErrorHandlerMiddleware> logger, RequestDelegate next)
    {
        _logger = logger;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            await HandleCustomExceptionResponseAsync(context, ex);
        }
    }

    private async Task HandleCustomExceptionResponseAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = MediaTypeNames.Application.Json;

        var response = new ErrorModel
        {
            ErrorType = ex.GetType().Name
        };

        switch (ex)
        {
            case OperationCanceledException:
                context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
                response.Message = "The request was canceled due to a timeout. Please try again with simpler parameters or try later.";
                _logger.LogWarning(ex, "Request was canceled or timed out: {Path}", context.Request.Path);
                break;
            case LanguageException langEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = $"Invalid language code: {langEx.Message}";
                break;

            case ArgumentException argEx when argEx.Message.Contains("Wikipedia page"):
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Message = $"Wikipedia page not found: {argEx.Message}";
                break;
            case ArgumentException argEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = $"Invalid argument: {argEx.Message}";
                break;
            case KeyNotFoundException:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Message = "The requested resource was not found.";
                break;
            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = "An unexpected error occurred while processing your request.";
                break;
        }

        response.StatusCode = context.Response.StatusCode;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(response, options);
        await context.Response.WriteAsync(json);
    }
}
