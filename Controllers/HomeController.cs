using Lebo.Factories;
using Lebo.Models.Generated;
using Lebo.Models.Pages;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace Lebo.Controllers
{
    public class HomeController : BaseRenderController<Home, HomeViewModel>
    {
        public HomeController(ILogger<RenderController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor, ViewModelFactoryResolver viewModelFactoryResolver)
            : base(logger, compositeViewEngine, umbracoContextAccessor, viewModelFactoryResolver)
        {
        }

        protected override Home Page => CurrentPage is Home page ? page : throw new InvalidOperationException(nameof(Page));
    }
}
