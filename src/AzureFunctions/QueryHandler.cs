using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SKFProductAssistant.Services;
using SKFProductAssistant.Models;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace SKFProductAssistant.AzureFunctions
{
    public class QueryHandler
    {
        private readonly QueryProcessor _queryProcessor;
        private readonly TelemetryClient _telemetryClient;

        public QueryHandler(QueryProcessor queryProcessor, TelemetryClient telemetryClient)
        {
            _queryProcessor = queryProcessor;
            _telemetryClient = telemetryClient;
        }

        [FunctionName("HandleQuery")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Processing user query.");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var userQuery = Newtonsoft.Json.JsonConvert.DeserializeObject<ProductQuery>(requestBody);

                if (userQuery == null || string.IsNullOrWhiteSpace(userQuery.Query))
                {
                    log.LogWarning("Invalid query input.");
                    _telemetryClient.TrackEvent("InvalidQueryInput", new Dictionary<string, string> { { "RequestBody", requestBody } });
                    return new BadRequestObjectResult("Invalid query input.");
                }

                var response = await _queryProcessor.ProcessQueryAsync(userQuery);

                if (response == null)
                {
                    log.LogWarning("Product or attribute not found.");
                    _telemetryClient.TrackEvent("ProductOrAttributeNotFound", new Dictionary<string, string> { { "Query", userQuery.Query } });
                    return new NotFoundObjectResult("Product or attribute not found.");
                }

                _telemetryClient.TrackEvent("QueryProcessedSuccessfully", new Dictionary<string, string> { { "Query", userQuery.Query } });
                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "An error occurred while processing the query.");
                _telemetryClient.TrackException(new ExceptionTelemetry(ex)
                {
                    SeverityLevel = SeverityLevel.Error,
                    Properties = { { "Function", "HandleQuery" } }
                });
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
