using Compact.Functions.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Compact.Functions
{
    public static class RouteProcessor
    {
        private static ILogger _logger;
        private static LinkProcessor _linkProcessor;
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

            var taskList = new List<Task>();

            foreach (var link in route.Links)
            {
                taskList.Add(_linkProcessor.ProcessAsync(route.Id, link));
            }

            Task.WaitAll(taskList.ToArray());

            await _storageManager.UpdateRouteFileAsync(name, route);

            _logger.LogInformation($"Route processed: {route.Id}");
        }

        private static void InitialiseDependencies(ILogger logger)
        {
            _logger = logger;
            _linkProcessor = new LinkProcessor(logger);
            _reportPoster = new RouteReportPoster(logger);
            _storageManager = new RouteStorageManager(logger);
        }
    }
}