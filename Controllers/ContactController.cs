using Lebo.Factories;
using Lebo.Models.Generated;
using Lebo.Models.Pages;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace Lebo.Controllers
{
    public class ContactController : BaseRenderController<Contact, ContactViewModel>
    {
        public ContactController(ILogger<RenderController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor, ViewModelFactoryResolver viewModelFactoryResolver) : base(logger, compositeViewEngine, umbracoContextAccessor, viewModelFactoryResolver)
        {
        }

        protected override Contact Page => CurrentPage is Contact page ? page : throw new InvalidOperationException(nameof(Page));
    }
}
