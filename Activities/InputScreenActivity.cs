using Newtonsoft.Json;
using WorkflowConfiguration.Interface;
using WorkflowConfiguration.Models;
using WorkflowConfigurator.Attributes;
using WorkflowConfigurator.Models;
using WorkflowConfigurator.Models.Activity;

namespace WorkflowConfiguration.Activities
{
    [DefaultDefinition("Activity used for User input")]
    [ActivityTitle("User Input Screen")]
    public class InputScreenActivity : Activity, IActivity
    {
        public string UserMessage { get; set; }

        public ActivityResult Execute(WorkflowContext workflowContext)
        {
            var result = new ActivityResult()
            {
                ResultOutput = UserMessage,
                ActivityState = new ActivityStateResult()
                {
                    ScreenId = "5",
                    Error = new Error { Code = 0, Message = "No error" },
                    Success = true,
                    ActivityMetadata = JsonConvert.SerializeObject(this),
                    ActivityType = nameof(InputScreenActivity)
                }
            };

            return Outcome(Outcomes.AWAIT_EXECUTE.ToString(), result);
        }

        public override ActivityResult Resume(WorkflowContext workflowContext)
        {
            if (!String.IsNullOrEmpty(workflowContext.ProcessAction.ActivityData.Hotkey))
            {
                return HandleHotkey(workflowContext);
            }

            workflowContext.WfInstance.SetLastOutput(workflowContext.ProcessAction.ActivityData.Input);

            var result = new ActivityResult()
            {
                ActivityState = new ActivityStateResult()
                {
                    Error = new Error { Code = 0, Message = "No error" },
                    Success = true,
                    ActivityMetadata = JsonConvert.SerializeObject(this),
                    ActivityType = nameof(InputScreenActivity),
                    Input = workflowContext.ProcessAction.ActivityData.Input
                }
            };

            return Done(result);
        }
    }
}
