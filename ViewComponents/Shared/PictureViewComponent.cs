using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Lebo.ViewComponents.Shared
{
    public class PictureViewComponent : ViewComponent
    {
        public Task<IViewComponentResult> InvokeAsync(IPublishedElement image)
        {
            return null;
        }

        private string? GetCropUrl(IPublishedContent image)
        {
            return image.GetCropUrl();
        }
    }
}
