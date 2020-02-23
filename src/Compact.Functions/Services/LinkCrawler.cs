using Compact.Models;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
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

            HttpClientHandler handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            var httpClient = new HttpClient(handler)
            {
                Timeout = new TimeSpan(0, 0, 20)
            };

            var responseMessage = await httpClient.GetAsync(link.Target);

            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Status Code: {responseMessage.StatusCode}");
            }

            string responseString = string.Empty;

            try
            {
                var response = await responseMessage.Content.ReadAsByteArrayAsync();
                var encodingString = responseMessage.Content.Headers.ContentType.CharSet;

                encodingString = encodingString.Equals("\"utf-8\"")
                    ? encodingString = "utf-8"
                    : encodingString;

                var pageEncoding = Encoding.GetEncoding(encodingString);
                responseString = pageEncoding.GetString(response, 0, response.Length - 1);

                var document = new HtmlDocument();
                document.LoadHtml(responseString);

                link.Title = document.DocumentNode.SelectSingleNode("html/head/title").InnerText;
            }
            catch (Exception ex)
            {
                link.Title = string.Empty;

                _logger.LogInformation($"Unable to determine page title from HTML: {responseString}, Exception: {ex.Message}");

                throw ex;
            }
            finally
            {
                _logger.LogInformation($"Applied Title: {link.Title}");
            }
        }
    }
}