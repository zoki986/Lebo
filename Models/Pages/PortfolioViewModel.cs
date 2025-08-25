namespace Lebo.Models.Pages
{
    public class PortfolioViewModel : PageViewModel
    {
        public IReadOnlyList<PortfolioImage> Images { get; set; } = [];
    }

    public class PortfolioImage
    {
        public required string CropedUrl { get; set; }
        public required string OriginalUrl { get; set; }
    }
}
