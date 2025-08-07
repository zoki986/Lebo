using Lebo.Models.Modules;

namespace Lebo.Models.Pages
{
    public class HomeViewModel : PageViewModel
    {
        public IEnumerable<BaseModule> Modules { get; set; } = [];
    }
}
