using Compact.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Compact.Functions.Services
{
    public class RouteReportPoster
    {
        private ILogger _logger;

        public RouteReportPoster(ILogger logger)
        {
            _logger = logger;
        }

        public async Task GenerateReportAsync(string routeId, string reason)
        {
            _logger.LogInformation($"Reporting dead link: {reason}");

            var apiBaseUrl = Environment.GetEnvironmentVariable("ApiBaseUrl");
            var httpClient = new HttpClient();

            var requestModel = new ReportRequestModel
            {
                RouteId = routeId
            };

            await httpClient.PostAsJsonAsync($"{apiBaseUrl}reports", requestModel);
        }
    }
}