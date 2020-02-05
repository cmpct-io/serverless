using Compact.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Compact.Functions.Services
{
    public class RouteStorageManager
    {
        private ILogger _logger;

        public RouteStorageManager(ILogger logger)
        {
            _logger = logger;
        }

        public RouteModel GetRoute(Stream routeStream)
        {
            StreamReader reader = new StreamReader(routeStream);

            string routeContent = reader.ReadToEnd();

            var result = JsonConvert.DeserializeObject<RouteModel>(routeContent);

            _logger.LogInformation($"Processing Route: {result.Id}");

            return result;
        }

        public async Task UpdateRouteFileAsync(string name, RouteModel route)
        {
            route.ProcessDate = DateTime.UtcNow;

            var storageConnectionString = Environment.GetEnvironmentVariable("StorageConnectionString");

            var azureStorageManager = new AzureStorageManager(storageConnectionString);

            await azureStorageManager.StoreObject("routes", name, route);
        }
    }
}