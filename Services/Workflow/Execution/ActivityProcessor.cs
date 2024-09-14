using WorkflowConfiguration.Activities;
using WorkflowConfiguration.Interface;
using WorkflowConfiguration.Models;
using WorkflowConfigurator.Activities;
using WorkflowConfigurator.Models.Activity;
using WorkflowConfigurator.Models.Workflow;

namespace WorkflowConfigurator.Services.Workflow.Execution
{
    public class ActivityProcessor
    {
        private readonly IActivityService _activityService;
        private readonly ILogger<ActivityProcessor> _logger;
        private MiniWorkflowService _miniWfService;

        public ActivityProcessor(ILogger<ActivityProcessor> logger, MiniWorkflowService miniWfService, IActivityService activityService)
        {
            _logger = logger;
            _miniWfService = miniWfService;
            _activityService = activityService;
        }

        public async Task<ActivityResult> ProcessActivity(WorkflowInstance workflowInstance,
            ActivityDefinition activity,
            ExecuteProcessAction executeProcessAction)
        {
            switch (workflowInstance.Status)
            {
                case WorkflowStatus.AWAIT_EXECUTE:
                    return await ResumeActivity(workflowInstance, activity, executeProcessAction);
                case WorkflowStatus.AWAIT_RESUME:
                    return await ResumeActivity(workflowInstance, activity, executeProcessAction);
                case WorkflowStatus.ONHOLD:
                    return await ResumeActivity(workflowInstance, activity, executeProcessAction);
                case WorkflowStatus.FINAL_ACTIVITY:
                    return ExecuteFinalActivity(workflowInstance, activity, executeProcessAction);
                case WorkflowStatus.FAULT:
                    return ExecuteFaultConfirmationActivity(workflowInstance, activity, executeProcessAction);

                default:
                    return await ExecuteActivity(workflowInstance, activity, executeProcessAction);
            }
        }

        private async Task<ActivityResult> ExecuteActivity(WorkflowInstance workflowInstance,
        ActivityDefinition activity,
        ExecuteProcessAction executeProcessAction)
        {
            var wfContext = new WorkflowContext(null, null, workflowInstance, executeProcessAction);

            ActivityResult activityResult = (_activityService.CreateInstanceByActivityType<IActivity>(
                activity)).Execute(wfContext);

            //_logger.LogInformation(
            //    $"Executing activity {workflowInstance.Id} " +
            //    $"with connection {activity.ActivityId} " + DateTime.Now);

            return activityResult;
        }

        private async Task<ActivityResult> ResumeActivity(WorkflowInstance workflowInstance,
            ActivityDefinition activity,
            ExecuteProcessAction executeProcessAction)
        {
            var wfContext = new WorkflowContext(null, null, workflowInstance, executeProcessAction);
            ActivityResult activityResult;
            //TODO after changing everything to be resolved from instance.CurrentState this is a workaround for ScanActivity,PrintAllActivity,ScanSPlitScreenActivity
            if (activity.ActivityType == "PrintAllLabelsActivity" || activity.ActivityType == "ScanActivity" || activity.ActivityType == "ScanSplitScreenActivity")
            {
                 activityResult = (_activityService.CreateInstanceByActivityTypeWorkaround<IActivity>(activity, workflowInstance))
                    .Resume(wfContext);
            }
            else
            {
                 activityResult = (_activityService.CreateInstanceByActivityType<IActivity>(
                    activity)).Resume(wfContext);
            }



            //_logger.LogInformation(
            //    $"Resuming activity {workflowInstance.Id} " +
            //    $"with connection {activity.ActivityId} " + DateTime.Now);

            return activityResult;
        }

        private ActivityResult ExecuteFinalActivity(WorkflowInstance workflowInstance,
            ActivityDefinition activity,
            ExecuteProcessAction executeProcessAction)
        {
            var finalResult = new ActivityResult();
            finalResult.Outcome = Outcomes.WF_COMPLETED.ToString();

            finalResult.ActivityState = new ActivityStateResult();
            finalResult.ActivityState.ScreenId = "2b";
            finalResult.ActivityState.ActivityMetadata = "{\"UserMessage\": \"You have finished the workflow\"}";
            finalResult.ActivityState.ActivityType = "MessageScreenActivity";
            finalResult.ActivityState.Error = new Error();
            finalResult.ActivityState.Success = true;

            return finalResult;

        }

        private ActivityResult ExecuteFaultConfirmationActivity(WorkflowInstance workflowInstance,
            ActivityDefinition activity,
            ExecuteProcessAction executeProcessAction)
        {
            var finalResult = new ActivityResult();
            finalResult.Outcome = Outcomes.FAULT.ToString();

            finalResult.ActivityState = new ActivityStateResult();
            finalResult.ActivityState.ScreenId = "2b";
            finalResult.ActivityState.ActivityMetadata = "{\"UserMessage\": \"Confirmed error\"}";
            finalResult.ActivityState.ActivityType = "MessageScreenActivity";
            finalResult.ActivityState.Error = new Error();
            finalResult.ActivityState.Success = true;

            return finalResult;

        }
    }
}
