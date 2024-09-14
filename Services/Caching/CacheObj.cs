namespace WorkflowConfigurator.Services.Caching
{
    public class CacheObj<T>
    {
        private T _data;

        public T Data
        {
            get
            {
                if (_data == null)
                {
                    LoadDataSync();
                }

                return _data;
            }
        }

        public DateTime? ExpiresAt { get; set; }

        private ICacheLoader<T> Loader { get; set; }

        public CacheObj(T data, DateTime? expireAt, ICacheLoader<T> loader)
        {
            _data = data;
            ExpiresAt = expireAt;
            Loader = loader;
        }

        public async Task LoadData()
        {
            _data = await Loader.Load();
        }

        public void LoadDataSync()
        {
            _data = Loader.LoadSync();
        }

        public void RefreshCache()
        {
            _data = default(T);
        }

        public delegate T FillCacheData();
    }
}
