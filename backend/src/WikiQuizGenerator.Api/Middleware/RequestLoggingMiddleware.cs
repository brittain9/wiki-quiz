using System.Diagnostics;

namespace WikiQuizGenerator.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                LogRequest(context, stopwatch.ElapsedMilliseconds);
            }
        }

        private void LogRequest(HttpContext context, long durationMs)
        {
            var statusCode = context.Response.StatusCode;
            var logLevel = GetLogLevel(statusCode, durationMs);

            _logger.Log(logLevel,
                "REQ {M} {P} => {SC} in {D}ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                durationMs);
        }

        private static LogLevel GetLogLevel(int statusCode, long durationMs)
        {
            return statusCode switch
            {
                >= 500 => LogLevel.Error,
                >= 400 => LogLevel.Warning,
                _ => durationMs > 500 ? LogLevel.Warning : LogLevel.Information
            };
        }
    }
}
