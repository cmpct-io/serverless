using Compact.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Compact.Functions.Services
{
    public class LinkCrawler
    {
        private ILogger _logger;

        public LinkCrawler(ILogger logger)
        {
            _logger = logger;
        }

        public async Task AppendLinkMetadata(LinkModel link)
        {
            _logger.LogInformation($"Scanning Link: {link.Target}");

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
                    link.Title = string.Empty;
                }
                finally
                {
                    _logger.LogInformation($"Applied Title: {link.Title}");
                }
            }
            else
            {
                throw new HttpRequestException($"Status Code: {responseMessage.StatusCode}");
            }
        }
    }
}