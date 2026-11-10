namespace Project.Core
{
    public interface ISaveService
    {
        T Load<T>(string key) where T : class, new();
        void Save<T>(string key, T data) where T : class;
    }
}
