using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System.Collections.Generic;

namespace SKFProductAssistant.Utils
{
    public class GlobalErrorHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalErrorHandler> _logger;

        public GlobalErrorHandler(RequestDelegate next, ILogger<GlobalErrorHandler> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new
            {
                message = "An unexpected error occurred. Please try again later.",
                details = exception.Message,
                timestamp = DateTime.UtcNow
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

        public static void HandleException(Exception exception, TelemetryClient telemetryClient, string source, string methodName)
        {
            telemetryClient.TrackException(exception, new Dictionary<string, string>
            {
                { "Source", source },
                { "Method", methodName }
            });
        }
    }
}
