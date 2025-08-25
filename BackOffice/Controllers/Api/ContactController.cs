using Microsoft.AspNetCore.Mvc;

namespace Lebo.BackOffice.Controllers.Api
{
    [ApiController]
    [Route("umbraco/backoffice/contact")]
    public class ContactController : Controller
    {
        private readonly IContactService _contactService;

        public ContactController(IContactService contactService)
        {
            _contactService = contactService;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int page = 1, int pageSize = 10)
        {
            try
            {
                var messages = await _contactService.GetPageAsync(page, pageSize, CancellationToken.None);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

    }
}
