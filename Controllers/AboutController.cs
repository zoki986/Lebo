using Lebo.Factories;
using Lebo.Models.Generated;
using Lebo.Models.Pages;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace Lebo.Controllers
{
    public class AboutController : BaseRenderController<About, AboutViewModel>
    {
        public AboutController(ILogger<RenderController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor, ViewModelFactoryResolver viewModelFactoryResolver) : base(logger, compositeViewEngine, umbracoContextAccessor, viewModelFactoryResolver)
        {
        }

        protected override About Page => CurrentPage is About about ? about : throw new InvalidOperationException();
    }
}
