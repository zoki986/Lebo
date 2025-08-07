using Lebo.Models.Interface;
using Lebo.Models.Pages;

namespace Lebo.Factories.Pages
{
    public abstract class BasePageViewModelFactory<TPage, TViewModel> : IViewModelFactory<TPage, TViewModel>
            where TViewModel : PageViewModel
            where TPage : IPage
    {
        public TViewModel CreateViewModel(TPage page)
        {
            var vm = Create(page);

            return vm;
        }

        protected abstract TViewModel Create(TPage page);
    }
}
