using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Lebo.Helpers.Picture
{
    public class HeroBannerPictureRenderer : IPictureRenderer
    {
        Dictionary<string, int> cropAliases = new()
        {
            { "HeroBanner_500", 500 },
            { "HeroBanner_800", 800 },
            { "HeroBanner_1080", 1080 },
            { "HeroBanner_1600", 1600 },
            { "HeroBanner_2000", 2000 },
            { "HeroBanner_2600", 2600 },
            { "HeroBanner_3200", 3200 },
            { "HeroBanner_3281", 3281 },
        };
        public IHtmlContent RenderPicture(IPublishedContent content)
        {


            return HtmlString.Empty;
        }
    }
}
