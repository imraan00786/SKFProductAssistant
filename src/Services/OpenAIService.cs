using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace SKFProductAssistant.Services
{
    public class OpenAIService
    {
        private readonly HttpClient _httpClient;
        private readonly TelemetryClient _telemetryClient;
        private readonly string _model;
        private readonly string _apiVersion;
        private readonly int _maxTokens;

        public OpenAIService(IConfiguration configuration, TelemetryClient telemetryClient)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (telemetryClient == null) throw new ArgumentNullException(nameof(telemetryClient));

            _model = configuration["OpenAI:Model"] ?? throw new ArgumentException("Model is not configured.");
            _apiVersion = configuration["OpenAI:ApiVersion"] ?? throw new ArgumentException("ApiVersion is not configured.");
            _maxTokens = int.TryParse(configuration["OpenAI:MaxTokens"], out var tokens) ? tokens : 50;

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(configuration["OpenAI:Endpoint"] ?? throw new ArgumentException("Endpoint is not configured."))
            };
            _httpClient.DefaultRequestHeaders.Add("api-key", configuration["OpenAI:ApiKey"] ?? throw new ArgumentException("ApiKey is not configured."));

            _telemetryClient = telemetryClient;
        }

        public async Task<(string product, string attribute)> ExtractProductAndAttributeAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) throw new ArgumentException("Query cannot be null or empty.", nameof(query));

            try
            {
                var prompt = new
                {
                    model = _model,
                    messages = new[]
                    {
                        new { role = "system", content = "Extract the product name and attribute from this query." },
                        new { role = "user", content = query }
                    },
                    max_tokens = _maxTokens
                };

                var content = new StringContent(JsonSerializer.Serialize(prompt), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"/openai/deployments/{_model}/chat/completions?api-version={_apiVersion}", content);
                response.EnsureSuccessStatusCode();

                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(responseBody);
                var text = result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

                var parts = text.Split(',');
                if (parts.Length >= 2)
                {
                    _telemetryClient.TrackEvent("OpenAIExtractionSuccess", new Dictionary<string, string> { { "Query", query } });
                    return (parts[0].Trim(), parts[1].Trim());
                }

                _telemetryClient.TrackEvent("OpenAIExtractionFailed", new Dictionary<string, string> { { "Query", query } });
                return (string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackException(new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error,
                    Properties = { { "Method", nameof(ExtractProductAndAttributeAsync) } }
                });
                throw;
            }
        }

        
    }
}
