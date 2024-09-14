using WorkflowConfigurator.Models.Activity;
using WorkflowConfigurator.Models.Workflow;

namespace WorkflowConfigurator.Services.Helper.Workflow
{
    /// <summary>
    /// Activity result to Workflow status Mapper
    /// </summary>
    public class ResultToWFStatus
    {
        public static WorkflowStatus MapToWFStatus(string outcome)
        {
            var dict = new Dictionary<string, WorkflowStatus>
            {
                { Outcomes.AWAIT_RESUME.ToString(), WorkflowStatus.AWAIT_RESUME },
                { Outcomes.AWAIT_EXECUTE.ToString(), WorkflowStatus.AWAIT_EXECUTE },
                { Outcomes.DONE.ToString(), WorkflowStatus.RUNNING },
                { Outcomes.ONHOLD.ToString(), WorkflowStatus.ONHOLD },
                { Outcomes.FINAL_ACTIVITY.ToString(), WorkflowStatus.FINAL_ACTIVITY },
                { Outcomes.FAULT.ToString(), WorkflowStatus.FAULT },
                { Outcomes.WF_COMPLETED.ToString(), WorkflowStatus.FINISHED },
            };

            if (!dict.ContainsKey(outcome))
            {
                return WorkflowStatus.RUNNING;
            }

            return dict[outcome];
        }
    }
}
