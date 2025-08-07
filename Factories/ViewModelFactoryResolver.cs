using Lebo.Factories.Pages;
using Lebo.Models.Interface;
using Lebo.Models.Modules;
using Lebo.Models.Pages;

namespace Lebo.Factories
{
    public class ViewModelFactoryResolver
    {
        private readonly IServiceProvider _services;

        public ViewModelFactoryResolver(IServiceProvider services)
        {
            _services = services;
        }

        public BaseModule CreateViewModel(IModule module)
        {
            var moduleType = module.GetType();
            var factoryType = typeof(IViewModelFactory<,>).MakeGenericType(moduleType, typeof(BaseModule));

            var factory = _services.GetRequiredKeyedService(factoryType, moduleType) as IViewModelFactory<IModule, BaseModule>
                ?? throw new ArgumentNullException($"No factory for module '{module.ContentType.Alias}' registered.");

            return factory.CreateViewModel(module);
        }

        public TViewModel CreateViewModel<TPage, TViewModel>(TPage page)
            where TPage : IPage
            where TViewModel : PageViewModel
        {
            var pageType = page.GetType();
            var factoryType = typeof(IViewModelFactory<,>).MakeGenericType(pageType, typeof(TViewModel));

            var factory = _services.GetRequiredKeyedService(factoryType, pageType);

            var typedFactory = (BasePageViewModelFactory<TPage, TViewModel>)factory;

            return typedFactory.CreateViewModel(page);
        }
    }
}
