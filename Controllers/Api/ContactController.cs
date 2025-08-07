using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.Common.Controllers;

namespace Lebo.Controllers.Api
{
    public class ContactController : UmbracoApiController
    {
        public IActionResult SendEmail()
        {


            return Ok();
        }
    }
}
