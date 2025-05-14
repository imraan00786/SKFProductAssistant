using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SKFProductAssistant.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using SKFProductAssistant.Utils;

namespace SKFProductAssistant.Services
{
    public class QueryProcessor
    {
        private readonly OpenAIService _openAIService;
        private readonly DatasheetService _datasheetService;
        private readonly CacheService _cacheService;
        private readonly TelemetryClient _telemetryClient;

        public QueryProcessor(OpenAIService openAIService, DatasheetService datasheetService, CacheService cacheService, TelemetryClient telemetryClient)
        {
            _openAIService = openAIService;
            _datasheetService = datasheetService;
            _cacheService = cacheService;
            _telemetryClient = telemetryClient;
        }

        public async Task<string> ProcessQueryAsync(ProductQuery query)
        {
            try
            {
                // Check cache first
                var cachedResponse = await _cacheService.GetCachedResponseAsync(query.Query);
                if (cachedResponse != null)
                {
                    _telemetryClient.TrackEvent("CacheHit", new Dictionary<string, string> { { "Query", query.Query } });
                    return cachedResponse;
                }

                // Use OpenAI to extract product and attribute
                var (product, attribute) = await _openAIService.ExtractProductAndAttributeAsync(query.Query);

                if (string.IsNullOrEmpty(product) || string.IsNullOrEmpty(attribute))
                {
                    _telemetryClient.TrackEvent("InvalidProductOrAttribute", new Dictionary<string, string> { { "Query", query.Query } });
                    return "I’m sorry, I can’t find that information.";
                }

                // Lookup datasheet
                var result = await _datasheetService.GetProductAttributeAsync(product, attribute);

                if (result == null)
                {
                    _telemetryClient.TrackEvent("AttributeNotFound", new Dictionary<string, string> { { "Product", product }, { "Attribute", attribute } });
                    return "I’m sorry, I can’t find that information.";
                }

                // Cache the response
                await _cacheService.CacheResponseAsync(query.Query, result);
                _telemetryClient.TrackEvent("QueryProcessedSuccessfully", new Dictionary<string, string> { { "Query", query.Query } });

                return result;
            }
            catch (Exception ex)
            {
                GlobalErrorHandler.HandleException(ex, _telemetryClient, nameof(QueryProcessor), nameof(ProcessQueryAsync));
                throw;
            }
        }
    }
}
