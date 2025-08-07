namespace Lebo.Factories
{
    public interface IViewModelFactory
    {
    }

    public interface IViewModelFactory<in TModel, out TViewModel> : IViewModelFactory
    {
        TViewModel CreateViewModel(TModel page);
    }
}
