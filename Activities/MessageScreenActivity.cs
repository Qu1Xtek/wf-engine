using Newtonsoft.Json;
using WorkflowConfiguration.Interface;
using WorkflowConfigurator.Attributes;
using WorkflowConfigurator.Models.Activity;
using WorkflowConfigurator.Util;

namespace WorkflowConfiguration.Activities
{
    [DefaultDefinition("Activity used to show a User Message")]
    [ActivityTitle("Screen Messaging")]
    public class MessageScreenActivity : Activity, IActivity
    {
        [DescriptionProp("The result of the previous Calculation activity can be used here by typing '<Y>', where Y is the result")]
        public string UserMessage { get; set; }

        public ActivityResult Execute(WorkflowContext workflowContext)
        {
            AddLastCalculationResultIFPlaceholderExists(workflowContext);
            var result = GetState();

            return Pause(result);
        }

        public override ActivityResult Resume(WorkflowContext workflowContext)
        {
            if (!String.IsNullOrEmpty(workflowContext.ProcessAction.ActivityData.Hotkey))
            {
                return HandleHotkey(workflowContext);
            }

            return Done(GetState());
        }


        private ActivityResult GetState()
        {
            return new ActivityResult()
            {
                ResultOutput = UserMessage,
                ActivityState = new ActivityStateResult()
                {
                    ScreenId = "2b",
                    Error = new Error { Code = 0, Message = "No error" },
                    Success = true,
                    ActivityMetadata = JsonConvert.SerializeObject(this),
                    ActivityType = nameof(MessageScreenActivity)
                }
            };
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
