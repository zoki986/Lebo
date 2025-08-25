using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;

namespace Lebo.Services
{
    public class PortfolioStats
    {
        public int TotalImages { get; set; }
        public Dictionary<string, int> CategoryCounts { get; set; } = new();
        public DateTime LastUpdated { get; set; }
    }

    public interface IPortfolioMediaService
    {
        List<PortfolioMediaService.PortfolioImage> GetAllPortfolioImages();
        Task<Models.Shared.PagedResult<PortfolioMediaService.PortfolioImage>> GetPortfolioImagesAsync(int page, int pageSize, string category = "all");
        Task<int> GetPortfolioImageCountAsync(string category = "all");
        Task<PortfolioStats> GetPortfolioStatsAsync();
    }

    public class PortfolioMediaService : IPortfolioMediaService
    {
        private readonly IUmbracoContext _umbracoContext;

        public PortfolioMediaService(
            IUmbracoContextFactory umbracoContextFactory,
            IUmbracoContextAccessor umbracoContextAccessor)
        {

            if (umbracoContextAccessor.TryGetUmbracoContext(out var existingContext))
            {
                _umbracoContext = existingContext;
            }
            else
            {
                _umbracoContext = umbracoContextFactory.EnsureUmbracoContext().UmbracoContext;
            }
        }

        public class PortfolioImage
        {
            public string Id { get; set; } = string.Empty;
            public string Url { get; set; } = string.Empty;
            public string OriginalUrl { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public string Alt { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public int SortOrder { get; set; }
            public DateTime CreateDate { get; set; }
            public DateTime UpdateDate { get; set; }
        }

        public List<PortfolioImage> GetAllPortfolioImages()
        {
            var allImages = new List<PortfolioImage>();

            // Get the media folders by name
            var fashionFolder = GetMediaFolderByName("Fashion & Portraits");
            var foodFolder = GetMediaFolderByName("Food & Beverage");

            // Process both folders
            allImages.AddRange(GetImagesFromFolder(fashionFolder, "fashion-portraits"));
            allImages.AddRange(GetImagesFromFolder(foodFolder, "food-beverage"));

            // Sort by sort order and then by name
            return allImages.OrderBy(x => x.SortOrder).ThenBy(x => x.Title).ToList();
        }

        public async Task<Models.Shared.PagedResult<PortfolioImage>> GetPortfolioImagesAsync(int page, int pageSize, string category = "all")
        {
            return await Task.Run(() =>
            {
                var allImages = GetAllPortfolioImages();

                // Filter by category if specified
                var filteredImages = category.ToLower() switch
                {
                    "fashion-portraits" => allImages.Where(x => x.Category == "fashion-portraits"),
                    "food-beverage" => allImages.Where(x => x.Category == "food-beverage"),
                    _ => allImages
                };

                var totalCount = filteredImages.Count();
                var items = filteredImages
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return new Models.Shared.PagedResult<PortfolioImage>
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            });
        }

        public async Task<int> GetPortfolioImageCountAsync(string category = "all")
        {
            return await Task.Run(() =>
            {
                var allImages = GetAllPortfolioImages();

                return category.ToLower() switch
                {
                    "fashion-portraits" => allImages.Count(x => x.Category == "fashion-portraits"),
                    "food-beverage" => allImages.Count(x => x.Category == "food-beverage"),
                    _ => allImages.Count
                };
            });
        }

        public async Task<PortfolioStats> GetPortfolioStatsAsync()
        {
            return await Task.Run(() =>
            {
                var allImages = GetAllPortfolioImages();

                var stats = new PortfolioStats
                {
                    TotalImages = allImages.Count,
                    LastUpdated = allImages.Any() ? allImages.Max(x => x.UpdateDate) : DateTime.MinValue,
                    CategoryCounts = new Dictionary<string, int>
                    {
                        ["all"] = allImages.Count,
                        ["fashion-portraits"] = allImages.Count(x => x.Category == "fashion-portraits"),
                        ["food-beverage"] = allImages.Count(x => x.Category == "food-beverage")
                    }
                };

                return stats;
            });
        }

        private IPublishedContent? GetMediaFolderByName(string folderName)
        {
            return _umbracoContext.Media?.GetAtRoot()?.FirstOrDefault(x => x.Name == folderName);
        }


        private List<PortfolioImage> GetImagesFromFolder(IPublishedContent? folder, string category)
        {
            if (folder == null) return new List<PortfolioImage>();

            var children =
                folder
                .Children<Models.Generated.File>()
                ?.Select(child => new PortfolioImage
                {
                    Id = child.Key.ToString(),
                    Url = GetImageUrl(child),
                    OriginalUrl = child.MediaUrl(),
                    Category = category,
                    Alt = child.Name,
                    Title = child.Name,
                    SortOrder = child.SortOrder,
                    CreateDate = child.CreateDate,
                    UpdateDate = child.UpdateDate
                }) ?? [];


            return children.ToList();
        }

        private string GetImageUrl(Models.Generated.File image)
        {
            var url = image.GetCropUrl("small");

            if (url.IsNullOrWhiteSpace())
            {
                url = image.MediaUrl();
            }

            return url;
        }
    }
}
