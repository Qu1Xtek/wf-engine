using WorkflowConfigurator.Interface.ActivityDefinition;
using WorkflowConfigurator.Models;
using WorkflowConfigurator.Models.Activity;
using WorkflowConfigurator.Repositories.Configurations;
using WorkflowConfigurator.Services.Caching;
using WorkflowConfigurator.Util;

namespace WorkflowConfigurator.Services.ActivityDefinitions
{
    public class ActivityDefinitionMaintainer : IActivityDefinitionRetriever
    {
        private readonly ILogger<ConfigurationsRepository> _logger;
        private readonly ConfigurationsRepository _configurationRepository;

        public ActivityDefinitionMaintainer(
            ILogger<ConfigurationsRepository> logger,
            ConfigurationsRepository configurationRepository)
        {
            _logger = logger;
            _configurationRepository = configurationRepository;
        }


        /// <summary>
        /// Returns only activities that belong to the passed company 
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public async Task<List<ActivityTemplate>> GetAllForCompany(string companyId)
        {
            var lowComp = companyId.ToLower();
            if (GlobalCache.ActivityTemplates.Data.ContainsKey(lowComp))
            {
                return GlobalCache.ActivityTemplates.Data[lowComp];
            }
            else
            {
                _logger.LogError($"Company {lowComp} does not contain settings for activity templates" +
                    $"thus will not be able to see any");

                return new List<ActivityTemplate>();
            }
        }

        /// <summary>
        /// Returns all the companies that we have in the configuration 
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public async Task<List<string>> GetMappedCompanies()
        {
            var mappedCompanies = GlobalCache.ActivityTemplates.Data.Keys
                .Where(c => c != StringConstants.ALL_COMPANY_ACTIVITIES)
                .ToList();

            return mappedCompanies;
        }

        /// <summary>
        /// Returns all activities, method should not be presented to use for customers
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns></returns>
        public async Task<List<ActivityTemplate>> GetAll()
        {
            return GlobalCache
                .ActivityTemplates
                .Data[StringConstants.ALL_COMPANY_ACTIVITIES];
        }

        /// <summary>
        /// Admin method, only used by admins to configure tempalte mapping
        /// </summary>
        /// <param name="companyMap"></param>
        /// <returns></returns>
        public async Task<GRes<ProjectSettings>> MapTempaltes(string company, string[] templates)
        {
            var editResult = await _configurationRepository
                .EditTemplateMap(company, templates);

            var result = new GRes<ProjectSettings>
            {
                Code = editResult == null ? 30 : 0,
                IsSuccesful = editResult == null ? false : true,
                Data = editResult
            };

            // Refresh cache after update
            if (result.IsSuccesful) GlobalCache.ActivityTemplates.RefreshCache();

            return result;
        }
    }
}
