using Microsoft.Extensions.Caching.Memory;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;

namespace Lebo.Services
{
    /// <summary>
    /// Service for managing portfolio cache invalidation
    /// </summary>
    public interface IPortfolioCacheService
    {
        void ClearPortfolioCache();
        void ClearPortfolioCacheForCategory(string category);
        void ClearMainPageCache();
        bool IsPortfolioMedia(IMedia media);
    }

    public class PortfolioCacheService : IPortfolioCacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<PortfolioCacheService> _logger;

        // Cache key patterns
        private const string CacheKeyPrefix = "portfolio_";
        private const string StatsKey = "portfolio_stats";
        private const string MainPageKey = "portfolio_main_page";
        private const string AllImagesKey = "portfolio_images_all";
        private const string FashionImagesKey = "portfolio_images_fashion-portraits";
        private const string FoodImagesKey = "portfolio_images_food-beverage";

        public PortfolioCacheService(IMemoryCache memoryCache, ILogger<PortfolioCacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public void ClearPortfolioCache()
        {
            try
            {
                var cacheKeys = GetAllPortfolioCacheKeys();

                foreach (var key in cacheKeys)
                {
                    _memoryCache.Remove(key);
                }

                _logger.LogInformation("Portfolio cache cleared completely. Removed {Count} cache entries.", cacheKeys.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing portfolio cache");
            }
        }

        public void ClearPortfolioCacheForCategory(string category)
        {
            try
            {
                var cacheKeys = GetCacheKeysForCategory(category);

                foreach (var key in cacheKeys)
                {
                    _memoryCache.Remove(key);
                }

                // Also clear stats cache and main page cache as category counts may have changed
                _memoryCache.Remove(StatsKey);
                _memoryCache.Remove(MainPageKey);

                _logger.LogInformation("Portfolio cache cleared for category '{Category}'. Removed {Count} cache entries.",
                    category, cacheKeys.Count + 2); // +2 for stats and main page
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing portfolio cache for category '{Category}'", category);
            }
        }

        public void ClearMainPageCache()
        {
            try
            {
                _memoryCache.Remove(MainPageKey);
                _logger.LogInformation("Portfolio main page cache cleared");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing portfolio main page cache");
            }
        }

        public bool IsPortfolioMedia(IMedia media)
        {
            if (media == null) return false;

            // Check if the media is in one of our portfolio folders
            var path = media.Path;
            var name = media.Name;

            // Check if it's directly in a portfolio folder or a descendant
            return IsInPortfolioFolder(media) || IsPortfolioImage(media);
        }

        private bool IsInPortfolioFolder(IMedia media)
        {
            // Check if media is in "Fashion & Portraits" or "Food & Beverage" folders
            var portfolioFolderNames = new[] { "Fashion & Portraits", "Food & Beverage" };

            // Walk up the parent chain to check if any parent is a portfolio folder
            var current = media;
            while (current != null)
            {
                if (portfolioFolderNames.Contains(current.Name ?? string.Empty))
                {
                    return true;
                }

                // Move to parent (this is a simplified check - in real implementation you'd need to get parent)
                break; // For now, just check the current item
            }

            return portfolioFolderNames.Contains(media.Name ?? string.Empty);
        }

        private bool IsPortfolioImage(IMedia media)
        {
            // Check if it's an image type in a portfolio-related path
            return media.ContentType.Alias == "Image" &&
                   (media.Path.Contains("Fashion") || media.Path.Contains("Food") ||
                    media.Name.Contains("Fashion") || media.Name.Contains("Food"));
        }

        private List<string> GetAllPortfolioCacheKeys()
        {
            var keys = new List<string>
            {
                StatsKey,
                MainPageKey,
                AllImagesKey,
                FashionImagesKey,
                FoodImagesKey
            };

            // Add paginated cache keys (first 10 pages, common page sizes)
            var categories = new[] { "all", "fashion-portraits", "food-beverage" };
            var pageSizes = new[] { 4, 8, 12, 16, 20 };

            foreach (var category in categories)
            {
                for (int page = 1; page <= 10; page++)
                {
                    foreach (var pageSize in pageSizes)
                    {
                        keys.Add($"portfolio_images_{category}_{page}_{pageSize}");
                    }
                }
            }

            return keys;
        }

        private List<string> GetCacheKeysForCategory(string category)
        {
            var keys = new List<string>();

            // Add category-specific keys
            if (category == "all")
            {
                keys.Add(AllImagesKey);
            }
            else if (category == "fashion-portraits")
            {
                keys.Add(FashionImagesKey);
            }
            else if (category == "food-beverage")
            {
                keys.Add(FoodImagesKey);
            }

            // Add paginated keys for this category
            var pageSizes = new[] { 4, 8, 12, 16, 20 };
            for (int page = 1; page <= 10; page++)
            {
                foreach (var pageSize in pageSizes)
                {
                    keys.Add($"portfolio_images_{category}_{page}_{pageSize}");
                }
            }

            return keys;
        }
    }

    /// <summary>
    /// Notification handler for media saved events to invalidate portfolio cache and warm images
    /// </summary>
    public class PortfolioMediaSavedNotificationHandler : INotificationAsyncHandler<MediaSavedNotification>
    {
        private readonly IPortfolioCacheService _cacheService;
        private readonly ILogger<PortfolioMediaSavedNotificationHandler> _logger;
        private readonly IConfiguration _configuration;

        public PortfolioMediaSavedNotificationHandler(
            IPortfolioCacheService cacheService,
            ILogger<PortfolioMediaSavedNotificationHandler> logger,
            IConfiguration configuration)
        {
            _cacheService = cacheService;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task HandleAsync(MediaSavedNotification notification, CancellationToken cancellationToken)
        {
            try
            {
                var portfolioMediaItems = notification.SavedEntities.Where(_cacheService.IsPortfolioMedia).ToList();

                if (portfolioMediaItems.Any())
                {
                    _logger.LogInformation("Portfolio media updated. Clearing cache for {Count} items.", portfolioMediaItems.Count);
                    _cacheService.ClearPortfolioCache();

                    // Auto-warm new/updated images if enabled
                    var autoWarmEnabled = _configuration.GetValue<bool>("ImageWarming:WarmOnMediaSave", true);
                    if (autoWarmEnabled)
                    {
                        // Run warming in background to avoid blocking the save operation
                        _ = Task.Run(async () => await WarmUpdatedImages(portfolioMediaItems), cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling media saved notification for portfolio cache");
            }
        }

        private async Task WarmUpdatedImages(List<IMedia> mediaItems)
        {
            try
            {
                _logger.LogInformation("🔥 Auto-warming {Count} updated portfolio images...", mediaItems.Count);

                var cropSizes = new[] { "small", "medium", "large" };
                var warmedCount = 0;

                // Create a single HttpClient for all images
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30);

                foreach (var media in mediaItems)
                {
                    try
                    {
                        var mediaUrl = media.GetValue("umbracoFile")?.ToString();
                        if (string.IsNullOrEmpty(mediaUrl)) continue;

                        foreach (var cropSize in cropSizes)
                        {
                            var cropUrl = GetImageCropUrl(mediaUrl, cropSize);
                            await httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, cropUrl));
                        }

                        warmedCount++;
                        _logger.LogDebug("✅ Auto-warmed updated image: {MediaName}", media.Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "⚠️ Failed to auto-warm updated image: {MediaName}", media.Name);
                    }
                }

                _logger.LogInformation("✅ Auto-warming completed: {WarmedCount} images processed", warmedCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during auto-warming of updated images");
            }
        }

        private string GetImageCropUrl(string originalUrl, string cropSize)
        {
            var separator = originalUrl.Contains('?') ? "&" : "?";
            return $"{originalUrl}{separator}crop={cropSize}";
        }
    }

    /// <summary>
    /// Notification handler for media deleted events to invalidate portfolio cache
    /// </summary>
    public class PortfolioMediaDeletedNotificationHandler : INotificationHandler<MediaDeletedNotification>
    {
        private readonly IPortfolioCacheService _cacheService;
        private readonly ILogger<PortfolioMediaDeletedNotificationHandler> _logger;

        public PortfolioMediaDeletedNotificationHandler(
            IPortfolioCacheService cacheService,
            ILogger<PortfolioMediaDeletedNotificationHandler> logger)
        {
            _cacheService = cacheService;
            _logger = logger;
        }

        public void Handle(MediaDeletedNotification notification)
        {
            try
            {
                var portfolioMediaItems = notification.DeletedEntities.Where(_cacheService.IsPortfolioMedia).ToList();

                if (portfolioMediaItems.Any())
                {
                    _logger.LogInformation("Portfolio media deleted. Clearing cache for {Count} items.", portfolioMediaItems.Count);
                    _cacheService.ClearPortfolioCache();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling media deleted notification for portfolio cache");
            }
        }
    }
}
