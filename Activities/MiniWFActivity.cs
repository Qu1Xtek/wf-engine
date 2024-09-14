using WorkflowConfiguration.Interface;
using WorkflowConfigurator.Attributes;
using WorkflowConfigurator.Models.Activity;

namespace WorkflowConfigurator.Activities
{
    [DefaultDefinition("MINIWF")]
    [ActivityTitle("MINIWF")]

    public class MiniWFActivity : Activity, IActivity
    {
        public string WfDefinitionId { get; set; }

        public ActivityResult Execute(WorkflowContext workflowContext)
        {
            throw new NotImplementedException("MINI WF should not have implementation and be executed");
        }
    }
}
