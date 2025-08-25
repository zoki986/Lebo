using Lebo.Services;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.BackgroundJobs;
using Umbraco.Extensions;

namespace Lebo.Services
{
    /// <summary>
    /// Umbraco background task for image warming with full Umbraco context
    /// </summary>
    public class ImageWarmingBackgroundTask : IRecurringBackgroundJob
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ImageWarmingBackgroundTask> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public ImageWarmingBackgroundTask(
            IServiceProvider serviceProvider,
            ILogger<ImageWarmingBackgroundTask> logger,
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// How often the task should run (every 6 hours by default)
        /// </summary>
        public TimeSpan Period => TimeSpan.FromHours(_configuration.GetValue<int>("ImageWarming:IntervalHours", 6));

        /// <summary>
        /// Delay before first execution (2 minutes by default)
        /// </summary>
        public TimeSpan Delay => TimeSpan.FromMinutes(_configuration.GetValue<int>("ImageWarming:StartupDelayMinutes", 2));

        /// <summary>
        /// Server roles that should run this job (Single and SchedulingPublisher by default)
        /// </summary>
        public ServerRole[] ServerRoles => new[] { ServerRole.Single, ServerRole.SchedulingPublisher };

        /// <summary>
        /// Event for period changes (not used in this implementation)
        /// </summary>
        public event EventHandler PeriodChanged { add { } remove { } }

        /// <summary>
        /// Task execution method with full Umbraco context available
        /// </summary>
        public async Task RunJobAsync()
        {
            var enabled = _configuration.GetValue<bool>("ImageWarming:Enabled", true);
            if (!enabled)
            {
                _logger.LogInformation("🔥 Image warming is disabled via configuration");
                return;
            }

            await WarmAllImagesAsync(CancellationToken.None);
        }

        private async Task WarmAllImagesAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("🔥 Starting background image warming task...");
                var startTime = DateTime.UtcNow;

                // Create scope to resolve scoped services properly
                using var scope = _serviceProvider.CreateScope();
                var portfolioMediaService = scope.ServiceProvider.GetRequiredService<IPortfolioMediaService>();

                // Full access to Umbraco services including IUmbracoContextAccessor
                var allImages = portfolioMediaService.GetAllPortfolioImages();

                if (!allImages.Any())
                {
                    _logger.LogInformation("📭 No portfolio images found to warm");
                    return;
                }

                var warmedCount = 0;
                var failedCount = 0;
                var cropSizes = new[] { "small", "medium", "large" };
                var concurrentRequests = _configuration.GetValue<int>("ImageWarming:ConcurrentRequests", 3);

                _logger.LogInformation("🎯 Warming {ImageCount} images with {CropCount} crop sizes using {ConcurrentRequests} concurrent requests",
                    allImages.Count, cropSizes.Length, concurrentRequests);

                // Create a single HttpClient for the entire batch
                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);

                // Process images with concurrency control and cancellation support
                var semaphore = new SemaphoreSlim(concurrentRequests);

                var tasks = allImages.Select(async image =>
                {
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    await semaphore.WaitAsync(cancellationToken);
                    try
                    {
                        await WarmSingleImageAsync(image, cropSizes, httpClient, cancellationToken);
                        Interlocked.Increment(ref warmedCount);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogInformation("🛑 Image warming cancelled");
                        throw;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "⚠️ Failed to warm image: {ImageTitle}", image.Title);
                        Interlocked.Increment(ref failedCount);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                await Task.WhenAll(tasks);

                var duration = DateTime.UtcNow - startTime;
                _logger.LogInformation("✅ Background image warming completed in {Duration:mm\\:ss}: {WarmedCount} warmed, {FailedCount} failed",
                    duration, warmedCount, failedCount);

                // Log performance statistics
                if (warmedCount > 0)
                {
                    var avgTimePerImage = duration.TotalMilliseconds / warmedCount;
                    _logger.LogInformation("📊 Performance: {AvgTime:F1}ms per image, {TotalCrops} total crops generated",
                        avgTimePerImage, warmedCount * cropSizes.Length);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("🛑 Background image warming task was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during background image warming task");
            }
        }

        private async Task WarmSingleImageAsync(PortfolioMediaService.PortfolioImage image, string[] cropSizes, HttpClient httpClient, CancellationToken cancellationToken)
        {
            // Get the actual media item to generate proper crop URLs
            using var scope = _serviceProvider.CreateScope();
            var portfolioService = scope.ServiceProvider.GetRequiredService<IPortfolioMediaService>();

            // Find the actual media item by ID to get proper crop URLs
            var mediaItem = await GetMediaItemById(image.Id, scope.ServiceProvider);
            if (mediaItem == null)
            {
                _logger.LogWarning("⚠️ Could not find media item for warming: {ImageId} - {ImageTitle}", image.Id, image.Title);
                return;
            }

            foreach (var cropSize in cropSizes)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    // Generate proper Umbraco crop URL (same as application uses)
                    var cropUrl = mediaItem.GetCropUrl(cropSize);

                    if (string.IsNullOrEmpty(cropUrl))
                    {
                        _logger.LogDebug("⚠️ No crop URL generated for {ImageTitle} - {CropSize}", image.Title, cropSize);
                        continue;
                    }

                    // Make a HEAD request to generate crop without downloading
                    using var request = new HttpRequestMessage(HttpMethod.Head, cropUrl);
                    var response = await httpClient.SendAsync(request, cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogDebug("✅ Warmed {ImageTitle} - {CropSize}: {CropUrl}", image.Title, cropSize, cropUrl);
                    }
                    else
                    {
                        _logger.LogDebug("⚠️ Failed to warm {ImageTitle} - {CropSize}: {StatusCode} - {CropUrl}",
                            image.Title, cropSize, response.StatusCode, cropUrl);
                    }
                }
                catch (OperationCanceledException)
                {
                    throw; // Re-throw cancellation
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "❌ Error warming {ImageTitle} - {CropSize}", image.Title, cropSize);
                    throw; // Re-throw to be caught by the calling method
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
