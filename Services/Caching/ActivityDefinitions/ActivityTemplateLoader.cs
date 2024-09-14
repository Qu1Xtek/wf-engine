using System.Reflection;
using WorkflowConfiguration.Interface;
using WorkflowConfigurator.Attributes;
using WorkflowConfigurator.Models.Activity;
using WorkflowConfigurator.Repositories.Configurations;
using WorkflowConfigurator.Services.Helper.Settings;
using WorkflowConfigurator.Util;

namespace WorkflowConfigurator.Services.Caching.ActivityDefinitions
{
    public class ActivityTemplateLoader : ICacheLoader<Dictionary<string, List<ActivityTemplate>>>
    {
        private IServiceProvider provider;
        private ConfigurationsRepository _confRepo;
        private readonly ILogger<ActivityTemplateLoader> _logger;
        readonly IConfiguration _conf;

        public ActivityTemplateLoader(IServiceProvider provider,
            ConfigurationsRepository cfr,
            ILogger<ActivityTemplateLoader> logger,
            IConfiguration conf)
        {
            this.provider = provider;
            _confRepo = cfr;
            _logger = logger;
            _conf = conf;
        }

        private string ApplyTemplateTitle(Type activityType)
        {
            var customAtt = activityType.GetCustomAttributes(
                typeof(ActivityTitleAttribute),
                true)
                .FirstOrDefault();

            return $"{customAtt ?? activityType.Name}";
        }

        private string ApplyTemplateDescription(Type activityType)
        {
            var customAtt = activityType.GetCustomAttributes(
                typeof(DefaultDefinitionAttribute),
                true)
                .FirstOrDefault();

            return customAtt?.ToString() ?? "";
        }

        private string[] ApplyOutcomes(Type activityType)
        {
            var customAtt = activityType.GetCustomAttributes(
                typeof(PossibleOutcomesAttribute),
                true)
                .FirstOrDefault();

            return $"{customAtt}".Split(',');
        }

        private List<ActivityProperty> ApplyProperties(PropertyInfo[] properties)
        {
            var activityProperties = new List<ActivityProperty>();

            foreach (var activityProperty in properties)
            {
                string? overwrittenType = activityProperty
                    .GetCustomAttributes(typeof(TypeOverwriteAttribute), true)
                    .FirstOrDefault()?
                    .ToString();

                string? predefinedValues = activityProperty
                    .GetCustomAttributes(typeof(PredifinedValuesAttribute), true)
                    .FirstOrDefault()?
                    .ToString();

                string? dataAddressValue = activityProperty
                    .GetCustomAttributes(typeof(DataAddressAttribute), true)
                    .FirstOrDefault()?
                    .ToString();

                string? description = activityProperty
                    .GetCustomAttributes(typeof(DescriptionPropAttribute), true)
                    .FirstOrDefault()?
                    .ToString();

                activityProperties.Add(new ActivityProperty
                {
                    Name = activityProperty.Name,
                    Type = overwrittenType ?? activityProperty.PropertyType.Name,
                    Value = predefinedValues ?? "",
                    DataAddress = !string.IsNullOrEmpty(dataAddressValue)
                        ? _conf.GetValue<string>(dataAddressValue)
                        : "",
                    Description = description ?? ""
                });
            }

            return activityProperties;
        }

        private IEnumerable<Type> GetAllTypes()
        {
            var aType = typeof(IActivity);
            var types = Assembly.GetExecutingAssembly().GetTypes();

            return types.Where(type
                    => aType.IsAssignableFrom(type) && !type.IsInterface)
                .ToList();
        }

        private List<ActivityTemplate> GetAll()
        {
            var allTemplate = new List<ActivityTemplate>();

            // Fill/init once per instance run/restart
            var activityTypes = GetAllTypes();

            foreach (var activityType in activityTypes)
            {
                ActivityTemplate activityTemplate = new ActivityTemplate
                {
                    Name = ApplyTemplateTitle(activityType),
                    Type = activityType.Name,
                    Description = ApplyTemplateDescription(activityType),
                    Outcomes = ApplyOutcomes(activityType),
                    Properties = ApplyProperties(activityType.GetProperties())
                };

                allTemplate.Add(activityTemplate);
            }

            return allTemplate;
        }

        /// Checks if the currently logged in user's company 
        /// has access to the activity 
        private bool NotAllowedForCompany(string companyName, string activityName)
        {
            // If not mapped/contains return true, means that comapny isn't allowed to see the activity
            return !(ActivityToCompanies.Mapping[companyName.ToLower()].Contains(activityName));
        }

        public async Task<Dictionary<string, List<ActivityTemplate>>> Load()
        {
            try
            {
                var allActivities = GetAll();

                var loadedData = new Dictionary<string, List<ActivityTemplate>>
                {
                    { StringConstants.ALL_COMPANY_ACTIVITIES, allActivities },
                };

                var projectSetting = await _confRepo!.GetFirstAsync();

                if (projectSetting != null)
                {
                    foreach (var item in projectSetting.CompanyActivities)
                    {
                        loadedData[item.Key.ToLower()] = allActivities
                            .Where(a => item.Value.Contains(a.Type))
                            .ToList();
                    }
                }

                return loadedData;

            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "ActivityTemplate cache was COULD NOT be filled successfully!");
            }

            return null;
        }

        public Dictionary<string, List<ActivityTemplate>> LoadSync()
        {
            return Load().GetAwaiter().GetResult();
        }
    }
}
