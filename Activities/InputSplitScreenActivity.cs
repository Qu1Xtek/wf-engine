using Newtonsoft.Json;
using WorkflowConfiguration.Interface;
using WorkflowConfigurator.Attributes;
using WorkflowConfigurator.Models.Activity;
using WorkflowConfigurator.Util;

namespace WorkflowConfiguration.Activities
{
    [DefaultDefinition("Activity used for User input")]
    [ActivityTitle("User Input (expanded screen)")]
    public class InputSplitScreenActivity : Activity, IActivity
    {
        public string UserMessage { get; set; }

        public ActivityResult Execute(WorkflowContext workflowContext)
        {
            AddLastCalculationResultIFPlaceholderExists(workflowContext);
            var result = new ActivityResult()
            {
                ResultOutput = UserMessage,
                ActivityState = new ActivityStateResult()
                {
                    ScreenId = "4",
                    Error = new Error { Code = 0, Message = "No error" },
                    Success = true,
                    ActivityMetadata = JsonConvert.SerializeObject(this),
                    ActivityType = nameof(InputSplitScreenActivity)
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
                    ActivityType = nameof(InputSplitScreenActivity),
                    Input = workflowContext.ProcessAction.ActivityData.Input
                }
            };

            return Done(result);
        }
        private void AddLastCalculationResultIFPlaceholderExists(WorkflowContext workflowContext)
        {
            if (UserMessage.Contains(StringConstants.CALCULATION_TEXT_PLACEHOLDER) && workflowContext.WfInstance.GlobalVariables.ContainsKey("LastCalculationResult"))
            {
                UserMessage = UserMessage.Replace(StringConstants.CALCULATION_TEXT_PLACEHOLDER, workflowContext.WfInstance.GlobalVariables["LastCalculationResult"].ToString());
            }
        }
    }
}
