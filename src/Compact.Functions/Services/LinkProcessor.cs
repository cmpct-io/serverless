using Compact.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Compact.Functions.Services
{
    public class LinkProcessor
    {
        private readonly ILogger _logger;
        private readonly LinkCrawler _linkCrawler;
        private readonly ScreenshotCaptureService _screenshotCapture;

        public LinkProcessor(ILogger logger)
        {
            _logger = logger;
            _linkCrawler = new LinkCrawler(logger);
            _screenshotCapture = new ScreenshotCaptureService(logger);
        }

        public async Task ProcessAsync(string routeId, LinkModel link)
        {
            try
            {
                await _linkCrawler.AppendLinkMetadata(link);
                await _screenshotCapture.CaptureScreenshotAsync(routeId, link);
            }
            catch (Exception ex)
            {
                // Pause automatic reports for now while the feature is considered
                // await _reportPoster.GenerateReportAsync(route.Id, ex.Message);

                _logger.LogInformation($"Failed to complete link processing: {ex.Message}");
            }
        }
    }
}
