using Compact.Functions.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Compact.Functions
{
    public static class RouteProcessor
    {
        private static ILogger _logger;
        private static LinkCrawler _linkCrawler;
        private static RouteReportPoster _reportPoster;
        private static RouteStorageManager _storageManager;

        [FunctionName("NewRouteProcessor")]
        public static async Task RunAsync([BlobTrigger("routes/{name}", Connection = "StorageConnectionString")] Stream routeStream, string name, ILogger logger)
        {
            InitialiseDependencies(logger);

            var route = _storageManager.GetRoute(routeStream);

            if (route.ProcessDate.HasValue)
            {
                _logger.LogInformation("Route has already been processed.");
                return;
            }

            foreach (var link in route.Links)
            {
                try
                {
                    await _linkCrawler.AppendLinkMetadata(link);
                }
                catch (Exception ex)
                {
                    // Pause automatic reports for now while the feature is considered
                    // await _reportPoster.GenerateReportAsync(route.Id, ex.Message);

                    _logger.LogInformation($"Unable to append metadata: {ex.Message}");
                }
            }

            await _storageManager.UpdateRouteFileAsync(name, route);

            _logger.LogInformation($"Route processed: {route.Id}");
        }

        private static void InitialiseDependencies(ILogger logger)
        {
            _logger = logger;
            _linkCrawler = new LinkCrawler(logger);
            _reportPoster = new RouteReportPoster(logger);
            _storageManager = new RouteStorageManager(logger);
        }
    }
}