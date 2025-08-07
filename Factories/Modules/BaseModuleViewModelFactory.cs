using Lebo.Models.Interface;
using Lebo.Models.Modules;

namespace Lebo.Factories.Modules
{
    public abstract class BaseModuleViewModelFactory<TModule, TViewModel> : IViewModelFactory<TModule, TViewModel>
        where TViewModel : BaseModule
        where TModule : IModule
    {
        public TViewModel CreateViewModel(TModule module)
        {
            var vm = Create(module);

            return vm;
        }

        protected abstract TViewModel Create(TModule module);
    }
}
