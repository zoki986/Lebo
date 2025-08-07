using Lebo.Factories;
using Lebo.Models.Generated;
using Lebo.Models.Interface;
using Lebo.Models.Pages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace Lebo.Controllers
{
    public abstract class BaseRenderController<TPage, TViewModel> : RenderController
        where TPage : IPage
        where TViewModel : PageViewModel
    {
        private readonly ViewModelFactoryResolver _viewModelFactoryResolver;

        public BaseRenderController(ILogger<RenderController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor, ViewModelFactoryResolver viewModelFactoryResolver)
            : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            _viewModelFactoryResolver = viewModelFactoryResolver;
        }

        public override IActionResult Index()
        {
            return CurrentTemplate(_viewModelFactoryResolver.CreateViewModel<TPage, TViewModel>(Page));
        }

        protected abstract TPage Page { get; }
    }
    public class HomeController : BaseRenderController<Home, HomeViewModel>
    {
        public HomeController(ILogger<RenderController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor, ViewModelFactoryResolver viewModelFactoryResolver)
            : base(logger, compositeViewEngine, umbracoContextAccessor, viewModelFactoryResolver)
        {
        }

        protected override Home Page => CurrentPage is Home page ? page : throw new InvalidOperationException(nameof(Page));

        //protected override IPage Page => CurrentPage is IPage page ? page : throw new InvalidOperationException(nameof(Page));
    }
}
