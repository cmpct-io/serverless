using Compact.Models;
using HtmlAgilityPack;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Compact.Functions
{
    public static class RouteProcessor
    {
        [FunctionName("NewRouteProcessor")]
        public static async Task RunAsync([BlobTrigger("routes/{name}", Connection = "StorageConnectionString")] Stream routeStream, string name, ILogger log)
        {
            var route = GetRoute(routeStream);
            log.LogInformation($"Detected new Route: {name}");

            if (route.ProcessDate.HasValue)
            {
                log.LogInformation("Route has already been processed.");
                return;
            }

            foreach (var link in route.Links)
            {
                SourceLinkMetadata(link);
                log.LogInformation($"Appended title: {link.Title} to link: {link.Target}");
            }

            route.ProcessDate = DateTime.UtcNow;

            await UpdateRouteFileAsync(name, route);
            log.LogInformation($"Route processed: {route.Id}");
        }

        private static RouteModel GetRoute(Stream routeStream)
        {
            StreamReader reader = new StreamReader(routeStream);

            string routeContent = reader.ReadToEnd();

            var result = JsonConvert.DeserializeObject<RouteModel>(routeContent);

            return result;
        }

        private static void SourceLinkMetadata(LinkModel link)
        {
            if (!link.Target.StartsWith("http"))
            {
                link.Target = $"https://{link.Target}";
            }

            var webGet = new HtmlWeb();
            var document = webGet.Load(link.Target);

            link.Title = document.DocumentNode.SelectSingleNode("html/head/title").InnerText;
        }

        private static async Task UpdateRouteFileAsync(string name, RouteModel route)
        {
            var storageConnectionString = System.Environment.GetEnvironmentVariable("StorageConnectionString");

            var azureStorageManager = new AzureStorageManager(storageConnectionString);

            await azureStorageManager.StoreObject("routes", name, route);
        }
    }
}