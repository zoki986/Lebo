using System.Reflection;

namespace Lebo.Factories
{
    public static class DependencyInjection
    {
        public static IUmbracoBuilder AddFactories(this IUmbracoBuilder builder)
        {
            builder.Services.AddSingleton<ViewModelFactoryResolver>();



            //MODULES
            //builder.Services.AddKeyedSingleton<IViewModelFactory, HeroBannerModuleFactory>(typeof(HeroBanner));



            //PAGES

            var factoryBaseType = typeof(IViewModelFactory);
            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => type.IsAssignableTo(factoryBaseType) && !type.IsInterface && !type.IsAbstract).ToList();

            foreach (var type in types)
            {
                var args = type.BaseType!.GetGenericArguments();
                var modelType = args[0];
                var viewModelType = args[1];
                var factoryType = typeof(IViewModelFactory<,>).MakeGenericType(modelType, viewModelType);
                builder.Services.AddKeyedSingleton(factoryType, args[0], type);
            }

            return builder;
        }
    }
}
