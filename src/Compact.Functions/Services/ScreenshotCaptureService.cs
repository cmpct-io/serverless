using Compact.Models;
using GrabzIt;
using GrabzIt.Enums;
using GrabzIt.Parameters;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Compact.Functions.Services
{
    public class ScreenshotCaptureService
    {
        private ILogger _logger;

        public ScreenshotCaptureService(ILogger logger)
        {
            _logger = logger;
        }

        public async Task CaptureScreenshotAsync(string routeId, LinkModel link)
        {
            try
            {
                bool enableScreenshots = bool.Parse(Environment.GetEnvironmentVariable("EnableScreenshots"));

                if (enableScreenshots)
                {
                    var applicationKey = Environment.GetEnvironmentVariable("GrabzitApplicationKey");
                    var applicationSecret = Environment.GetEnvironmentVariable("GrabzitApplicationSecret");
                    GrabzItClient grabzIt = new GrabzItClient(applicationKey, applicationSecret);

                    var imageOptions = new ImageOptions
                    {
                        Format = ImageFormat.jpg
                    };

                    grabzIt.URLToImage(link.Target, imageOptions);

                    var file = grabzIt.SaveTo();

                    var azManager = new AzureStorageManager(Environment.GetEnvironmentVariable("StorageConnectionString"));

                    var screenshotFileName = $"{routeId}-{Guid.NewGuid()}.jpg";

                    await azManager.StoreFile("screenshots", screenshotFileName, file.Bytes);

                    link.ScreenshotFileName = screenshotFileName;

                    _logger.LogInformation($"Saved Link Screenshot: {screenshotFileName}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to process screenshot: {ex.Message}");

                throw ex;
            }
        }
    }
}