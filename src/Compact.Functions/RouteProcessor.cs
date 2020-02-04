using Compact.Models;
using HtmlAgilityPack;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Compact.Functions
{
    public static class RouteProcessor
    {
        private static ILogger _logger;

        [FunctionName("NewRouteProcessor")]
        public static async Task RunAsync([BlobTrigger("routes/{name}", Connection = "StorageConnectionString")] Stream routeStream, string name, ILogger log)
        {
            _logger = log;

            var route = GetRoute(routeStream);
            log.LogInformation($"Detected new Route: {name}");

            if (route.ProcessDate.HasValue)
            {
                log.LogInformation("Route has already been processed.");
                return;
            }

            foreach (var link in route.Links)
            {
                try
                {
                    log.LogInformation($"Scanning Link: {link.Target}");
                    await SourceLinkMetadataAsync(link);
                    log.LogInformation($"Applied Title: {link.Title}");
                }
                catch (Exception ex)
                {
                    var exceptionType = ex.GetType();
                    log.LogInformation($"Reporting dead link: {ex.Message}");
                    await GenerateReportAsync(route.Id);
                }
            }

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

        private static async Task SourceLinkMetadataAsync(LinkModel link)
        {
            if (!link.Target.StartsWith("http"))
            {
                link.Target = $"https://{link.Target}";
            }

            var httpClient = new HttpClient();
            httpClient.Timeout = new TimeSpan(0, 0, 20);

            var responseMessage = await httpClient.GetAsync(link.Target);

            if (responseMessage.IsSuccessStatusCode)
            {
                string responseString = string.Empty;

                try
                {
                    var response = await responseMessage.Content.ReadAsByteArrayAsync();
                    var encoding = responseMessage.Content.Headers.ContentType.CharSet;
                    if ("UTF-8".Equals(encoding, StringComparison.OrdinalIgnoreCase) || "\"utf-8\"".Equals(encoding, StringComparison.OrdinalIgnoreCase) || encoding == null)
                    {
                        responseString = Encoding.UTF8.GetString(response, 0, response.Length - 1);
                    }
                    else
                    {
                        responseString = Encoding.Unicode.GetString(response, 0, response.Length - 1);
                    }
                    

                    var document = new HtmlDocument();
                    document.LoadHtml(responseString);

                    link.Title = document.DocumentNode.SelectSingleNode("html/head/title").InnerText;
                }
                catch (Exception ex)
                {
                    _logger.LogInformation($"Unable to determine page title from HTML: {responseString}, Exception: {ex.Message}");
                    link.Title = "Unable to fetch Page Title";
                }
            }
            else
            {
                throw new HttpRequestException($"Status Code: {responseMessage.StatusCode}");
            }
        }

        private static async Task UpdateRouteFileAsync(string name, RouteModel route)
        {
            route.ProcessDate = DateTime.UtcNow;

            var storageConnectionString = Environment.GetEnvironmentVariable("StorageConnectionString");

            var azureStorageManager = new AzureStorageManager(storageConnectionString);

            await azureStorageManager.StoreObject("routes", name, route);
        }

        private static async Task GenerateReportAsync(string routeId)
        {
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