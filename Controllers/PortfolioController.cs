using Lebo.Models.Pages;
using Lebo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Caching.Memory;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace Lebo.Controllers
{
    public class PortfolioController : RenderController
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IPortfolioCacheService _cacheService;
        private readonly ILogger<PortfolioController> _portfolioLogger;
        private const int CacheDurationMinutes = 30;
        private const string MainPortfolioCacheKey = "portfolio_main_page";

        public PortfolioController(
            ILogger<RenderController> logger,
            ICompositeViewEngine compositeViewEngine,
            IUmbracoContextAccessor umbracoContextAccessor,
            IMemoryCache memoryCache,
            IPortfolioCacheService cacheService,
            ILogger<PortfolioController> portfolioLogger)
            : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            _memoryCache = memoryCache;
            _cacheService = cacheService;
            _portfolioLogger = portfolioLogger;
        }

        [ResponseCache(Duration = CacheDurationMinutes * 60, VaryByHeader = "User-Agent", Location = ResponseCacheLocation.Any)]
        public override IActionResult Index()
        {
            try
            {
                // Try to get cached model first
                var model = _memoryCache.GetOrCreate(MainPortfolioCacheKey, entry =>
                {
                    // Set cache options
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheDurationMinutes);
                    entry.SetPriority(CacheItemPriority.High); // High priority since this is the main page
                    entry.SetSlidingExpiration(TimeSpan.FromMinutes(10)); // Extend cache if accessed within 10 minutes

                    _portfolioLogger.LogDebug("Creating new portfolio main page cache entry");

                    // Create the model
                    var portfolioModel = new PortfolioViewModel()
                    {
                        Images = UmbracoContext
                            .Media
                            .GetAtRoot()
                            .SelectMany(m => m.Children())
                            .Select(x => new PortfolioImage
                            {
                                CropedUrl = x.GetCropUrl("small"),
                                OriginalUrl = x.Url()
                            })
                            .ToList()
                    };

                    _portfolioLogger.LogInformation("Portfolio main page loaded with {ImageCount} images", portfolioModel.Images.Count);
                    return portfolioModel;
                });

                // Set response cache headers
                Response.Headers["Cache-Control"] = $"public, max-age={CacheDurationMinutes * 60}";
                Response.Headers["Vary"] = "Accept-Encoding";

                return CurrentTemplate(model);
            }
            catch (Exception ex)
            {
                _portfolioLogger.LogError(ex, "Error loading portfolio main page");

                // Fallback to non-cached version if caching fails
                var fallbackModel = new PortfolioViewModel()
                {
                    Images = new List<PortfolioImage>()
                };

                return CurrentTemplate(fallbackModel);
            }
        }
    }
}
