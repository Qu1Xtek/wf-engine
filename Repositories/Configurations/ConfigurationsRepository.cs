using MongoDB.Driver;
using WorkflowConfigurator.Models;
using WorkflowConfigurator.Repositories.Interface;
using WorkflowConfigurator.Services.Caching;
using WorkflowConfigurator.Services.Helper.Settings;

namespace WorkflowConfigurator.Repositories.Configurations
{
    public class ConfigurationsRepository : BaseMongoRepository<ProjectSettings>, IConfigurationRepository
    {
        private ILogger<ConfigurationsRepository> _logger;
        // This lock is used to achieve atomicy updating the p
        private static object _lock = new object();

        public ConfigurationsRepository(IConfiguration config, ILogger<ConfigurationsRepository> logger) : base(config)
        {
            _logger = logger;
        }

        protected override string GetCollectionName(IConfiguration config)
        {
            return config
                .GetSection(Configs.Collections)
                .GetValue<string>(Configs.WFConfiguration);
        }

        public async Task<ProjectSettings> GetFirstAsync()
        {
            return await _collection
                .Find(_ => true)
                .Limit(1)
                .FirstOrDefaultAsync();
        }

        public async Task<ProjectSettings> EditTemplateMap(string company, string[] mapping)
        {
            try
            {
                lock (_lock)
                {
                    var setting = _collection
                        .Find(_ => true)
                        .Limit(1)
                        .FirstOrDefaultAsync()
                        .GetAwaiter()
                        .GetResult();


                    // Ensure that settings is initialized
                    setting ??= new ProjectSettings(); // Initialize settings 
                    setting.CompanyActivities ??= new Dictionary<string, string[]>(); // Initialize template mapping

                    setting.CompanyActivities[company] = mapping;

                    var upd = Builders<ProjectSettings>
                        .Update
                        .Set(s => s.CompanyActivities, setting.CompanyActivities);

                    _collection.UpdateOne(s => s.CompanyActivities != null, upd, new UpdateOptions { IsUpsert = true });

                    return setting;
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, $"Error while editing templates mapping {mapping}");
                return null;
            }
            
        }

    }
}
