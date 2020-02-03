using Compact.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace Compact.Functions
{
    public static class RouteProcessor
    {
        [FunctionName("NewRouteProcessor")]
        public static void Run([BlobTrigger("tests/{name}", Connection = "StorageConnectionString")] Stream routeStream, string name, ILogger log)
        {
            log.LogInformation($"Detected new Route: {name}");

            var route = GetRoute(routeStream);

            log.LogInformation(route.Id);
            log.LogInformation(route.Links.First().Target);
        }

        private static RouteModel GetRoute(Stream routeStream)
        {
            StreamReader reader = new StreamReader(routeStream);

            string routeContent = reader.ReadToEnd();

            var result = JsonConvert.DeserializeObject<RouteModel>(routeContent);

            return result;
        }
    }
}