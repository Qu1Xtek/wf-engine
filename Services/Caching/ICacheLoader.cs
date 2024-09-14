namespace WorkflowConfigurator.Services.Caching
{
    public interface ICacheLoader<T>
    {
        public Task<T> Load();
        public T LoadSync();
    }
}
