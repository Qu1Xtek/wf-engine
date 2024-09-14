using Newtonsoft.Json;
using WorkflowConfiguration.Interface;
using WorkflowConfigurator.Attributes;
using WorkflowConfigurator.Models.Activity;

namespace WorkflowConfiguration.Activities
{
    [DefaultDefinition("Activity that provides a logic split point based on User choice")]
    [PossibleOutcomes("FALSE, TRUE")]
    [ActivityTitle("Conditional Message")]
    public class IfScreenMessageActivity : Activity, IActivity
    {
        public string UserMessage { get; set; }

        public ActivityResult Execute(WorkflowContext workflowContext)
        {
            var result = new ActivityResult()
            {
                ResultOutput = UserMessage,
                ActivityState = new ActivityStateResult()
                {
                    ScreenId = "7",
                    Error = new Error { Code = 0, Message = "No error" },
                    Success = true,
                    ActivityMetadata = JsonConvert.SerializeObject(this),
                    ActivityType = nameof(IfScreenMessageActivity),
                }
            };


            return Pause(result);
        }

        public override ActivityResult Resume(WorkflowContext workflowContext)
        {
            if (!String.IsNullOrEmpty(workflowContext.ProcessAction.ActivityData.Hotkey))
            {
                return HandleHotkey(workflowContext);
            }

            var input = workflowContext.ProcessAction.ActivityData.ActionData.ToUpper();

            var result = new ActivityResult()
            {
                ResultOutput = input,
                ActivityState = new ActivityStateResult()
                {
                    Error = new Error { Code = 0, Message = "No error" },
                    Success = true,
                    ActivityMetadata = JsonConvert.SerializeObject(this),
                    ActivityType = nameof(IfScreenMessageActivity),
                    Input = input
                }
            };

            return Outcome(input, result);            
        }
    }
}
