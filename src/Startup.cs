using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using SKFProductAssistant.Services;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;

[assembly: FunctionsStartup(typeof(SKFProductAssistant.Startup))]

namespace SKFProductAssistant
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var logger = builder.Services.BuildServiceProvider().GetService<ILogger<Startup>>();
            logger?.LogInformation("Configuring services in Startup class.");

            // Register services
            builder.Services.AddSingleton<OpenAIService>();
            builder.Services.AddSingleton<DatasheetService>();
            builder.Services.AddSingleton<CacheService>();
            builder.Services.AddSingleton<TelemetryClient>();
            builder.Services.AddSingleton<QueryProcessor>();
        }
    }
}
