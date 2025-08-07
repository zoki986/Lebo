using Microsoft.AspNetCore.Html;
using Umbraco.Cms.Core.Models.PublishedContent;

namespace Lebo.Helpers.Picture
{
    public interface IPictureRenderer
    {
        IHtmlContent RenderPicture(IPublishedContent content);
    }
}
