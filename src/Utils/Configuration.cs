using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace SKFProductAssistant.Utils
{
    public static class Configuration
    {
        public static IConfiguration GetConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }
    }

    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalErrorHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<GlobalErrorHandler>();
        }
    }
}
