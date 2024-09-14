using WorkflowConfigurator.Models.Activity;
using WorkflowConfigurator.Services.Caching.ActivityDefinitions;

namespace WorkflowConfigurator.Services.Caching
{
    /// <summary>
    /// Global cache of the project, should be used as a singleton instance 
    /// registered in the DI
    /// </summary>
    /// 
    public class GlobalCache : IHostedService
    {
        private IServiceProvider _provider;
        private IServiceScope _scope;


        private void InitCache()
        {
            var templateLoader = _scope
                .ServiceProvider
                .GetService<ActivityTemplateLoader>();

            ActivityTemplates = new CacheObj<Dictionary<string, List<ActivityTemplate>>>(null, null, templateLoader);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            InitCache();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public GlobalCache(IServiceProvider provider)
        {
            _provider = provider;
            _scope = _provider.CreateScope();

            
        }

        public static CacheObj<Dictionary<string, List<ActivityTemplate>>> ActivityTemplates { get; private set; }
    }
}
