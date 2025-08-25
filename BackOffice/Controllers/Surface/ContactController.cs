using Lebo.Models.Contact;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;

namespace Lebo.BackOffice.Controllers.Surface
{
    public class ContactController : SurfaceController
    {
        private readonly IContactService _contactService;
        public ContactController(IUmbracoContextAccessor umbracoContextAccessor,
                                 IUmbracoDatabaseFactory databaseFactory,
                                 ServiceContext services,
                                 AppCaches appCaches,
                                 IProfilingLogger profilingLogger,
                                 IPublishedUrlProvider publishedUrlProvider,
                                 IContactService contactService)
                    : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _contactService = contactService;
        }

        [HttpPost]
        public async Task<IActionResult> Submit([FromForm] ContactMessageDto dto)
        {
            // anti-spam: honeypot
            if (!string.IsNullOrWhiteSpace(dto.Website))
            {
                ModelState.AddModelError("", "There was a problem with your submission.");
                return CurrentUmbracoPage();
            }

            // anti-spam: time trap
            if (dto.FormRenderedAt is null)
            {
                ModelState.AddModelError("", "There was a problem with your submission.");
                return CurrentUmbracoPage();
            }
            var delta = DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeMilliseconds(dto.FormRenderedAt.Value);
            if (delta < TimeSpan.FromSeconds(3) || delta > TimeSpan.FromHours(1))
            {
                ModelState.AddModelError("", "There was a problem with your submission.");
                return CurrentUmbracoPage();
            }

            // FluentValidation has already populated ModelState
            if (!ModelState.IsValid) return CurrentUmbracoPage();

            await _contactService.AddMessageAsync(dto);

            TempData["ContactMessageSuccess"] = true;

            return RedirectToCurrentUmbracoPage();
        }
    }
}
