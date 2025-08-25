namespace Lebo.Models.Pages
{
    public abstract class PageViewModel
    {
        public string CurrentPageAlias { get; set; } = string.Empty;
        public NavigationState Navigation { get; set; } = new NavigationState();
    }

    public class NavigationState
    {
        public bool IsHome { get; set; }
        public bool IsPortfolio { get; set; }
        public bool IsAbout { get; set; }
        public bool IsContact { get; set; }
    }
}
