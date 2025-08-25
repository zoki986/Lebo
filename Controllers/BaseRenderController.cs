using Lebo.Factories;
using Lebo.Models.Pages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace Lebo.Controllers
{
    public abstract class BaseRenderController<TPage, TViewModel> : RenderController
        where TPage : IPublishedContent
        where TViewModel : PageViewModel, new()
    {
        private readonly ViewModelFactoryResolver _viewModelFactoryResolver;

        public BaseRenderController(ILogger<RenderController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor, ViewModelFactoryResolver viewModelFactoryResolver)
            : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            _viewModelFactoryResolver = viewModelFactoryResolver;
        }

        public override IActionResult Index()
        {
            var viewModel = new TViewModel();
            return CurrentTemplate(viewModel);
        }

        protected abstract TPage Page { get; }
    }
}
