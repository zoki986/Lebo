//using Lebo.Models.Generated;
//using Lebo.Models.Interface;
//using Lebo.Models.Pages;

//namespace Lebo.Factories.Pages
//{
//    public class HomeViewModelFactory : BasePageViewModelFactory<Home, HomeViewModel>
//    {
//        private readonly ViewModelFactoryResolver _factoryResolver;
//        public HomeViewModelFactory(ViewModelFactoryResolver factoryResolver)
//        {
//            _factoryResolver = factoryResolver;
//        }
//        protected override HomeViewModel Create(Home page)
//        {
//            var vm = new HomeViewModel();

//            vm.Modules = page.Modules
//                ?.Select(m => m.Content)
//                .OfType<IHomeModule>()
//                .Select(_factoryResolver.CreateViewModel) ?? [];

//            return vm;
//        }

//    }
//}
