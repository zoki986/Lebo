namespace Lebo.Models.Modules
{
    public class BaseModule
    {
        public string ViewName
        {
            get
            {
                var typeName = GetType().Name;
                return typeName.Remove(typeName.IndexOf("ViewModel") + 1);
            }
        }
    }
}
