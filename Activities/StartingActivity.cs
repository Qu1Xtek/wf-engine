using WorkflowConfiguration.Interface;
using WorkflowConfigurator.Attributes;
using WorkflowConfigurator.Models.Activity;

namespace WorkflowConfigurator.Activities
{
    [DefaultDefinition("Starting acitivty")]
    public class StartingActivity : Activity, IActivity
    {
        public ActivityResult Execute(WorkflowContext workflowContext)
        {
            throw new NotImplementedException();
        }
    }
}
