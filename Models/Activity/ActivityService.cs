using WorkflowConfiguration.Activities;
using Newtonsoft.Json;
using WorkflowConfigurator.Activities;
using WorkflowConfigurator.Models.Workflow;
using WorkflowConfiguration.Interface;
using WorkflowConfigurator.Attributes;
using System.Reflection;
using WorkflowConfigurator.Activities.ScanSpace;

namespace WorkflowConfigurator.Models.Activity
{
    public class ActivityService : IActivityService
    {
        public Dictionary<string, Type> ActivityMapping => InitiateActionParserMapping();
        private readonly IConfiguration _configuration;

        public ActivityService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public T CreateInstanceByActivityType<T>(ActivityDefinition activityDefinition)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
             
            var activityInstance = JsonConvert.DeserializeObject(activityDefinition.ActivityMetadata, ActivityMapping[activityDefinition.ActivityType], settings);
            var res = (T)activityInstance;
            return res;
        }
        public T CreateInstanceByActivityTypeWorkaround<T>(ActivityDefinition activityDefinition, WorkflowInstance workflowInstance)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            var activityInstance = JsonConvert.DeserializeObject(workflowInstance.CurrentState.ActivityMetadata, ActivityMapping[activityDefinition.ActivityType], settings);
            var res = (T)activityInstance;
            return res;
        }

        public Dictionary<string, Type> InitiateActionParserMapping()
        {
            var mapping = new Dictionary<string, Type>();
            mapping.Add("ScanActivity", typeof(ScanActivity));
            mapping.Add("ScanSplitScreenActivity", typeof(ScanSplitScreenActivity));
            mapping.Add(nameof(ScanActivityNew), typeof(ScanActivityNew)); 
            mapping.Add(nameof(LinkedScanCheckActivity), typeof(LinkedScanCheckActivity));
            mapping.Add("IfScreenMessageActivity", typeof(IfScreenMessageActivity));
            mapping.Add("IfScreenLastOutcome", typeof(IfScreenLastOutcome));
            mapping.Add("CalculationActivity", typeof(CalculationActivity));
            mapping.Add("InputScreenActivity", typeof(InputScreenActivity));
            mapping.Add("InputSplitScreenActivity", typeof(InputSplitScreenActivity));
            mapping.Add("InputWithAddonsScreenActivity", typeof(InputWithAddonsScreenActivity));
            mapping.Add("MessageScreenActivity", typeof(MessageScreenActivity));
            mapping.Add("TimerActivity", typeof(TimerActivity));
            mapping.Add("UserConfigurableTimerActivity", typeof(UserConfigurableTimerActivity));
            mapping.Add("PrintAllLabelsActivity", typeof(PrintAllLabelsActivity));
            mapping.Add(nameof(MiniWFActivity), typeof(MiniWFActivity));



            return mapping;
        }


        public List<ActivityTemplate> GenerateActivitiesResponse()
        {

            var myType = typeof(IActivity);
            var activityTypes = System.Reflection.Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(type => typeof(IActivity).IsAssignableFrom(type) && !type.IsInterface);

            var result = new List<ActivityTemplate>();

                foreach (var activityType in activityTypes)
                {
                    //if (IgnoreActivity(activityType))
                    //{
                    //    // Skip adding iterration, this activity must be ignored
                    //    continue;
                    //}

                    ActivityTemplate activityTemplate = new ActivityTemplate();

                    activityTemplate.Name = ApplyTemplateTitle(activityType);

                    activityTemplate.Type = activityType.Name;
                    activityTemplate.Description = ApplyTemplateDescription(activityType);
                    activityTemplate.Outcomes = ApplyOutcomes(activityType);
                    activityTemplate.Properties = ApplyProperties(activityType.GetProperties());

                    result.Add(activityTemplate);
                }

            return result;
        }

        private bool IgnoreActivity(Type activityType)
        {
            var ignoreList = new List<string>
            {
                nameof(InputSplitScreenActivity),
                nameof(ScanSplitScreenActivity)
            };

            if (ignoreList.Contains(activityType.Name))
            {
                return true;
            }

            return false;
        }

        private string ApplyTemplateTitle(Type activityType)
        {
            var customAtt = activityType.GetCustomAttributes(typeof(ActivityTitleAttribute), true).FirstOrDefault();
            return customAtt == null ? activityType.Name : customAtt.ToString();
        }

        private string ApplyTemplateDescription(Type activityType)
        {
            var customAtt = activityType.GetCustomAttributes(typeof(DefaultDefinitionAttribute), true).FirstOrDefault();
            return customAtt.ToString();
        }

        private string[] ApplyOutcomes(Type activityType)
        {
            var customAtt = activityType.GetCustomAttributes(typeof(PossibleOutcomesAttribute), true).FirstOrDefault();
            return customAtt.ToString().Split(',');
        }

        private List<ActivityProperty> ApplyProperties(PropertyInfo[] properties)
        {
            var activityProperties = new List<ActivityProperty>();

            foreach (var activityProperty in properties)
            {
                string? overwrittenType = activityProperty
                    .GetCustomAttributes(typeof(TypeOverwriteAttribute), true)
                    .FirstOrDefault()?.ToString();

                string? predefinedValues = activityProperty
                    .GetCustomAttributes(typeof(PredifinedValuesAttribute), true)
                    .FirstOrDefault()?.ToString();

                string? dataAddressValue = activityProperty
                    .GetCustomAttributes(typeof(DataAddressAttribute), true)
                    .FirstOrDefault()?.ToString();

                string? description = activityProperty
                    .GetCustomAttributes(typeof(DescriptionPropAttribute), true)
                    .FirstOrDefault()?.ToString();

                activityProperties.Add(new ActivityProperty
                {
                    Name = activityProperty.Name,
                    Type = overwrittenType ?? activityProperty.PropertyType.Name,
                    Value = predefinedValues ?? "",
                    DataAddress = !string.IsNullOrEmpty(dataAddressValue) ? _configuration.GetValue<string>(dataAddressValue) : "",
                    Description = description
                });
            }

            return activityProperties;
        }
    }
}
