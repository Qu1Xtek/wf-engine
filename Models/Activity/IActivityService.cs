using WorkflowConfiguration.Interface;
using WorkflowConfigurator.Models.Workflow;

namespace WorkflowConfigurator.Models.Activity
{
    public interface IActivityService
    {
        T CreateInstanceByActivityType<T>(ActivityDefinition activityDefinition);
        T CreateInstanceByActivityTypeWorkaround<T>(ActivityDefinition activityDefinition, WorkflowInstance workflowInstance);
    }
}