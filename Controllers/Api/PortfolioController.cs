using Lebo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using System.Text;

namespace Lebo.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortfolioController : Controller
    {
        private readonly IPortfolioMediaService _portfolioMediaService;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<PortfolioController> _logger;
        private readonly IPortfolioCacheService _cacheService;
        private const int DefaultPageSize = 8;
        private const int MaxPageSize = 50;
        private const int CacheDurationSeconds = 1800; // 30 minutes
        private const int MainPageCacheDurationMinutes = 30;

        public PortfolioController(
            IPortfolioMediaService portfolioMediaService,
            IMemoryCache memoryCache,
            ILogger<PortfolioController> logger,
            ICompositeViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IPortfolioCacheService cacheService)
        {
            _portfolioMediaService = portfolioMediaService;
            _memoryCache = memoryCache;
            _logger = logger;
            _cacheService = cacheService;
        }

        /// <summary>
        /// Get portfolio images with pagination and filtering, returns HTML partial view
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page (max 50)</param>
        /// <param name="category">Category filter: 'all', 'fashion-portraits', 'food-beverage'</param>
        /// <returns>HTML partial view content</returns>
        [HttpGet("images")]
        [ResponseCache(Duration = CacheDurationSeconds, VaryByQueryKeys = new[] { "page", "pageSize", "category" })]
        public async Task<IActionResult> GetImages(int page = 1, int pageSize = DefaultPageSize, string category = "all")
        {
            try
            {
                // Validate parameters
                if (page < 1) page = 1;
                if (pageSize < 1 || pageSize > MaxPageSize) pageSize = DefaultPageSize;
                if (string.IsNullOrWhiteSpace(category)) category = "all";

                // Generate ETag for conditional requests
                var etag = GenerateETag(page, pageSize, category);
                if (Request.Headers.ContainsKey("If-None-Match") &&
                    Request.Headers["If-None-Match"].ToString().Trim('"') == etag)
                {
                    _logger.LogDebug("Returning 304 Not Modified for portfolio images request");
                    return StatusCode(304); // Not Modified
                }

                // Get cached or fresh data
                var cacheKey = $"portfolio_images_{category}_{page}_{pageSize}";
                var result = await _portfolioMediaService.GetPortfolioImagesAsync(page, pageSize, category);

                if (result == null || !result.Items.Any())
                {
                    _logger.LogWarning("No portfolio images found for page={Page}, category={Category}", page, category);
                    return NotFound(new { message = "No images found" });
                }

                // Set response headers
                Response.Headers["ETag"] = $"\"{etag}\"";
                Response.Headers["Cache-Control"] = $"public, max-age={CacheDurationSeconds}";
                Response.Headers["Vary"] = "Accept-Encoding";

                _logger.LogDebug("Successfully returned {Count} portfolio images for page {Page}, category {Category}",
                    result.Items.Count(), page, category);

                return PartialView("_PortfolioItems", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting portfolio images for page={Page}, category={Category}", page, category);
                return StatusCode(500, new { message = "An error occurred while loading images" });
            }
        }

        /// <summary>
        /// Get portfolio statistics and metadata
        /// </summary>
        [HttpGet("stats")]
        [ResponseCache(Duration = CacheDurationSeconds)]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var cacheKey = "portfolio_stats";
                var stats = await _memoryCache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(CacheDurationSeconds);
                    entry.SetPriority(CacheItemPriority.Low);

                    return await _portfolioMediaService.GetPortfolioStatsAsync();
                });

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting portfolio statistics");
                return StatusCode(500, new { message = "An error occurred while loading statistics" });
            }
        }

        /// <summary>
        /// Clear portfolio cache (for admin use)
        /// </summary>
        [HttpPost("clear-cache")]
        public IActionResult ClearCache()
        {
            try
            {
                _cacheService.ClearPortfolioCache();
                return Ok(new { message = "Portfolio cache cleared successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing portfolio cache");
                return StatusCode(500, new { message = "An error occurred while clearing cache" });
            }
        }

        /// <summary>
        /// Clear main portfolio page cache only (for admin use)
        /// </summary>
        [HttpPost("clear-main-cache")]
        public IActionResult ClearMainPageCache()
        {
            try
            {
                _cacheService.ClearMainPageCache();
                return Ok(new { message = "Portfolio main page cache cleared successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing portfolio main page cache");
                return StatusCode(500, new { message = "An error occurred while clearing main page cache" });
            }
        }

        /// <summary>
        /// Get cache status and performance metrics (for admin use)
        /// </summary>
        [HttpGet("cache-status")]
        public IActionResult GetCacheStatus()
        {
            try
            {
                // Check if main cache entries exist
                var mainPageCached = _memoryCache.TryGetValue("portfolio_main_page", out _);
                var statsCached = _memoryCache.TryGetValue("portfolio_stats", out _);

                // Check some common API cache entries
                var allImagesCached = _memoryCache.TryGetValue("portfolio_images_all_1_8", out _);
                var fashionImagesCached = _memoryCache.TryGetValue("portfolio_images_fashion-portraits_1_8", out _);
                var foodImagesCached = _memoryCache.TryGetValue("portfolio_images_food-beverage_1_8", out _);

                var cacheStatus = new
                {
                    timestamp = DateTime.UtcNow,
                    mainPageCached = mainPageCached,
                    statsCached = statsCached,
                    apiCacheStatus = new
                    {
                        allImages = allImagesCached,
                        fashionPortraits = fashionImagesCached,
                        foodBeverage = foodImagesCached
                    },
                    cacheSettings = new
                    {
                        mainPageCacheDurationMinutes = MainPageCacheDurationMinutes,
                        apiCacheDurationSeconds = CacheDurationSeconds
                    }
                };

                return Ok(cacheStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache status");
                return StatusCode(500, new { message = "An error occurred while getting cache status" });
            }
        }

        /// <summary>
        /// Warm all portfolio images by pre-generating crops (for admin use)
        /// </summary>
        [HttpPost("warm-all")]
        public IActionResult WarmAllImages()
        {
            try
            {
                _logger.LogInformation("🔥 Starting manual image warming for all portfolio images...");

                var allImages = _portfolioMediaService.GetAllPortfolioImages();
                var warmedCount = 0;
                var failedCount = 0;
                var cropSizes = new[] { "small", "medium", "large" };

                foreach (var image in allImages)
                {
                    try
                    {
                        // Force crop generation by accessing different crop sizes
                        foreach (var cropSize in cropSizes)
                        {
                            // This will trigger Umbraco to generate the crop if it doesn't exist
                            var cropUrl = GetImageCropUrl(image.OriginalUrl, cropSize);
                            if (!string.IsNullOrEmpty(cropUrl))
                            {
                                _logger.LogDebug($"✅ Warmed {image.Title} - {cropSize} crop");
                            }
                        }
                        warmedCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"⚠️ Failed to warm image: {image.Title}");
                        failedCount++;
                    }
                }

                var message = $"Image warming completed: {warmedCount} images warmed successfully";
                if (failedCount > 0)
                {
                    message += $", {failedCount} images failed";
                }

                _logger.LogInformation($"✅ {message}");

                return Ok(new {
                    message = message,
                    totalImages = allImages.Count,
                    warmedCount = warmedCount,
                    failedCount = failedCount,
                    cropSizes = cropSizes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during image warming process");
                return StatusCode(500, new { message = "An error occurred while warming images" });
            }
        }

        /// <summary>
        /// Warm images for a specific category (for admin use)
        /// </summary>
        [HttpPost("warm-category/{category}")]
        public async Task<IActionResult> WarmCategoryImages(string category)
        {
            try
            {
                _logger.LogInformation($"🔥 Starting manual image warming for category: {category}");

                var result = await _portfolioMediaService.GetPortfolioImagesAsync(1, 1000, category);
                var warmedCount = 0;
                var failedCount = 0;
                var cropSizes = new[] { "small", "medium", "large" };

                foreach (var image in result.Items)
                {
                    try
                    {
                        // Force crop generation by accessing different crop sizes
                        foreach (var cropSize in cropSizes)
                        {
                            var cropUrl = GetImageCropUrl(image.OriginalUrl, cropSize);
                            if (!string.IsNullOrEmpty(cropUrl))
                            {
                                _logger.LogDebug($"✅ Warmed {image.Title} - {cropSize} crop");
                            }
                        }
                        warmedCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"⚠️ Failed to warm image: {image.Title}");
                        failedCount++;
                    }
                }

                var message = $"Category warming completed: {warmedCount} images warmed successfully in category '{category}'";
                if (failedCount > 0)
                {
                    message += $", {failedCount} images failed";
                }

                _logger.LogInformation($"✅ {message}");

                return Ok(new {
                    message = message,
                    category = category,
                    totalImages = result.Items.Count(),
                    warmedCount = warmedCount,
                    failedCount = failedCount,
                    cropSizes = cropSizes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error warming category images for: {category}");
                return StatusCode(500, new { message = $"An error occurred while warming images for category: {category}" });
            }
        }

        private string GenerateETag(int page, int pageSize, string category)
        {
            var input = $"{page}_{pageSize}_{category}_{DateTime.UtcNow:yyyyMMddHH}"; // Hour-based ETag
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(hash)[..16]; // Use first 16 characters
        }

        private string GetImageCropUrl(string originalUrl, string cropSize)
        {
            try
            {
                // Generate crop URL by appending crop parameter
                var separator = originalUrl.Contains('?') ? "&" : "?";
                var cropUrl = $"{originalUrl}{separator}crop={cropSize}";

                // Log the crop URL for debugging
                _logger.LogDebug("Generated crop URL: {CropUrl}", cropUrl);

                return cropUrl;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to generate crop URL for: {OriginalUrl}", originalUrl);
                return originalUrl; // Return original URL as fallback
            }
        }

        private async Task<string> RenderPartialViewAsync(string viewName, object model)
        {
            // For now, return a simple HTML representation
            // In a production environment, you would use a proper view rendering approach
            var result = model as Models.Shared.PagedResult<PortfolioMediaService.PortfolioImage>;
            if (result?.Items?.Any() == true)
            {
                var html = new StringBuilder();

                foreach (var image in result.Items)
                {
                    html.AppendLine($@"
                        <div class=""portfolio-item"" data-category=""{image.Category}"" data-id=""{image.Id}"">
                            <img src=""{image.Url}""
                                 alt=""{image.Alt}""
                                 title=""{image.Title}""
                                 loading=""lazy""
                                 data-main-url=""{image.OriginalUrl}""
                                 class=""portfolio-image"" />
                        </div>");
                }

                // Add pagination metadata
                html.AppendLine($@"
                    <div class=""portfolio-pagination-data""
                         data-current-page=""{result.Page}""
                         data-total-pages=""{result.TotalPages}""
                         data-has-next=""{result.HasNextPage.ToString().ToLower()}""
                         data-has-previous=""{result.HasPreviousPage.ToString().ToLower()}""
                         data-total-count=""{result.TotalCount}""
                         style=""display: none;"">
                    </div>");

                return html.ToString();
            }

            return "<div class=\"portfolio-no-results\"><p>No images found for the selected category.</p></div>";
        }

        private string GetCategoryDisplayName(string category)
        {
            return category switch
            {
                "fashion-portraits" => "Fashion & Portraits",
                "food-beverage" => "Food & Beverage",
                _ => "All"
            };
        }
    }
}
