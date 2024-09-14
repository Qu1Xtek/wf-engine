using WorkflowConfigurator.Models.Activity;

namespace WorkflowConfigurator.Models.Response
{
    public class WorkflowInvokerResponse
    {
        public ActivityStateResult ActivityState { get; set; }

        public WorkflowInvokerResponse(ActivityStateResult activityState)
        {
            this.ActivityState = activityState;
        }
    }
}
