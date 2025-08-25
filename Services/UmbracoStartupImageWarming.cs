using Lebo.Services;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Lebo.Services
{
    /// <summary>
    /// Handles Umbraco application started event to warm images on startup
    /// This has full access to Umbraco context including IUmbracoContextAccessor
    /// </summary>
    public class UmbracoStartupImageWarmingHandler : INotificationAsyncHandler<UmbracoApplicationStartedNotification>
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UmbracoStartupImageWarmingHandler> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public UmbracoStartupImageWarmingHandler(
            IServiceProvider serviceProvider,
            ILogger<UmbracoStartupImageWarmingHandler> logger,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task HandleAsync(UmbracoApplicationStartedNotification notification, CancellationToken cancellationToken)
        {
            var enabled = _configuration.GetValue<bool>("ImageWarming:StartupWarmingEnabled", true);

            if (!enabled)
            {
                _logger.LogInformation("🔥 Startup image warming is disabled via configuration");
                return;
            }

            // Run warming in background with proper delay and cancellation support
            try
            {
                // Wait for the configured delay
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    return;

                await WarmAllImagesOnStartup();
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("🛑 Startup image warming cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during startup image warming");
            }
        }

        private async Task WarmAllImagesOnStartup()
        {
            try
            {
                _logger.LogInformation("🔥 Starting Umbraco startup image warming...");
                var startTime = DateTime.UtcNow;

                // Create scope to resolve scoped services properly
                using var scope = _serviceProvider.CreateScope();
                var portfolioMediaService = scope.ServiceProvider.GetRequiredService<IPortfolioMediaService>();

                // Full access to Umbraco services including IUmbracoContextAccessor
                var allImages = portfolioMediaService.GetAllPortfolioImages();

                if (!allImages.Any())
                {
                    _logger.LogInformation("📭 No portfolio images found to warm on startup");
                    return;
                }

                var warmedCount = 0;
                var failedCount = 0;
                var cropSizes = new[] { "small", "medium", "large" };
                var concurrentRequests = _configuration.GetValue<int>("ImageWarming:ConcurrentRequests", 3);

                _logger.LogInformation("🎯 Startup warming {ImageCount} images with {CropCount} crop sizes",
                    allImages.Count, cropSizes.Length);

                // Create a single HttpClient for the entire batch
                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);

                // Process images with concurrency control
                var semaphore = new SemaphoreSlim(concurrentRequests);

                var tasks = allImages.Select(async image =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        await WarmSingleImageAsync(image, cropSizes, httpClient);
                        Interlocked.Increment(ref warmedCount);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "⚠️ Failed to warm image on startup: {ImageTitle}", image.Title);
                        Interlocked.Increment(ref failedCount);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                await Task.WhenAll(tasks);

                var duration = DateTime.UtcNow - startTime;
                _logger.LogInformation("✅ Startup image warming completed in {Duration:mm\\:ss}: {WarmedCount} warmed, {FailedCount} failed",
                    duration, warmedCount, failedCount);

                // Log performance statistics
                if (warmedCount > 0)
                {
                    var avgTimePerImage = duration.TotalMilliseconds / warmedCount;
                    _logger.LogInformation("📊 Startup performance: {AvgTime:F1}ms per image, {TotalCrops} total crops generated",
                        avgTimePerImage, warmedCount * cropSizes.Length);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during startup image warming");
            }
        }

        private async Task WarmSingleImageAsync(PortfolioMediaService.PortfolioImage image, string[] cropSizes, HttpClient httpClient)
        {
            // Get the actual media item to generate proper crop URLs
            using var scope = _serviceProvider.CreateScope();

            // Find the actual media item by ID to get proper crop URLs
            var mediaItem = await GetMediaItemById(image.Id, scope.ServiceProvider);
            if (mediaItem == null)
            {
                _logger.LogWarning("⚠️ Could not find media item for startup warming: {ImageId} - {ImageTitle}", image.Id, image.Title);
                return;
            }

            foreach (var cropSize in cropSizes)
            {
                try
                {
                    // Generate proper Umbraco crop URL (same as application uses)
                    var cropUrl = mediaItem.GetCropUrl(cropSize);

                    if (string.IsNullOrEmpty(cropUrl))
                    {
                        _logger.LogDebug("⚠️ No crop URL generated for startup warming {ImageTitle} - {CropSize}", image.Title, cropSize);
                        continue;
                    }

                    // Use HEAD request to generate crop without downloading
                    using var request = new HttpRequestMessage(HttpMethod.Head, cropUrl);
                    var response = await httpClient.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogDebug("✅ Startup warmed {ImageTitle} - {CropSize}: {CropUrl}", image.Title, cropSize, cropUrl);
                    }
                    else
                    {
                        _logger.LogDebug("⚠️ Failed to startup warm {ImageTitle} - {CropSize}: {StatusCode} - {CropUrl}",
                            image.Title, cropSize, response.StatusCode, cropUrl);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "❌ Error startup warming {ImageTitle} - {CropSize}", image.Title, cropSize);
                }
            }
        }

        private Task<IPublishedContent?> GetMediaItemById(string mediaId, IServiceProvider serviceProvider)
        {
            try
            {
                if (!Guid.TryParse(mediaId, out var guid))
                    return Task.FromResult<IPublishedContent?>(null);

                var umbracoContextFactory = serviceProvider.GetRequiredService<IUmbracoContextFactory>();

                using var contextReference = umbracoContextFactory.EnsureUmbracoContext();
                var umbracoContext = contextReference.UmbracoContext;

                var result = umbracoContext.Media?.GetById(guid);
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting media item by ID: {MediaId}", mediaId);
                return Task.FromResult<IPublishedContent?>(null);
            }
        }
    }
}
