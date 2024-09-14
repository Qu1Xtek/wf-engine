using Newtonsoft.Json;
using WorkflowConfiguration.Interface;
using WorkflowConfigurator.Attributes;
using WorkflowConfigurator.Models.Activity;
using WorkflowConfigurator.Services.Printer;
using WorkflowConfigurator.Util;

namespace WorkflowConfiguration.Activities
{
    [DefaultDefinition("Activity used to print all of the labels needed for the Workflow")]
    [ActivityTitle("Label Printing")]
    public class PrintAllLabelsActivity : Activity, IActivity
    {
        private PrinterService _printerService;

        [JsonProperty]
        private string _currentMessageToShow = string.Empty;
        [JsonProperty]
        private string _currentActionToShow = string.Empty;
        [JsonProperty]
        private string _scanImageToShow = string.Empty;
        [JsonProperty]
        private string _iconToShow = string.Empty;
        [JsonProperty]
        private string _currentState = string.Empty;


        public PrintAllLabelsActivity()
        {
            using (var scope = Provider.CreateScope())
            {
                _printerService = scope.ServiceProvider.GetService<PrinterService>();
            }
        }
        public ActivityResult Execute(WorkflowContext workflowContext)
        {
            _currentMessageToShow = "Please scan printer Id";
            _currentActionToShow = "Scan";
            _scanImageToShow = "Cryotube";
            _iconToShow = "scan";
            _currentState = ScreenStates.SCAN.ToString();

            var result = new ActivityResult()
            {
                ResultOutput = _currentMessageToShow,
                ActivityState = new ActivityStateResult()
                {
                    ScreenId = "2a",
                    Error = new Error { Code = 0, Message = "No error" },
                    Success = true,
                    ActivityMetadata = JsonConvert.SerializeObject(this),
                    ActivityType = nameof(PrintAllLabelsActivity)
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

            if (workflowContext.ProcessAction.ActivityData?.Confirm == "confirm" && _currentState.Equals(ScreenStates.MESSAGE.ToString()))
            {
                _currentMessageToShow = "Printer confirmed";

                var resultCOnfirm = new ActivityResult()
                {
                    ResultOutput = "Printer confirmed",
                    ActivityState = new ActivityStateResult()
                    {
                        Error = new Error { Code = 0, Message = "No error" },
                        Success = true,
                        ActivityMetadata = JsonConvert.SerializeObject(this),
                        ActivityType = nameof(PrintAllLabelsActivity),
                        Input = workflowContext.ProcessAction.ActivityData?.Confirm
                    }
                };

                var base64BarcodeStrings = _printerService.CreateLabelsPdf(workflowContext.WfInstance);
                // Send thr print job thourgh PrintNode
                var printJobResults = _printerService.SendPrintJob(
                    base64BarcodeStrings,
                    long.Parse(workflowContext.WfInstance.GlobalVariables["PrinterId"]));

                //return Done(GetState());

                return Done(resultCOnfirm);
            }

            var scannedValue = workflowContext.ProcessAction.ActivityData.ScannedValue;
            workflowContext.WfInstance.GlobalVariables["PrinterId"] = scannedValue;

            workflowContext.WfInstance.SetLastOutput(workflowContext.ProcessAction.ActivityData.ScannedValue);
            _currentMessageToShow = "Confirm Scan";
            _currentActionToShow = "Confirm";
            _iconToShow = "confirm";
            _currentState = ScreenStates.MESSAGE.ToString();

            var result = new ActivityResult()
            {
                ResultOutput = "Scanned printer id arrived",
                ActivityState = new ActivityStateResult()
                {
                    ScreenId = "2b",
                    Error = new Error { Code = 0, Message = "No error" },
                    Success = true,
                    ActivityMetadata = JsonConvert.SerializeObject(this),
                    ActivityType = nameof(PrintAllLabelsActivity),
                    Input = workflowContext.ProcessAction.ActivityData.ScannedValue
                }
            };

            return Pause(result);
        }


        private ActivityResult GetState()
        {
            return new ActivityResult()
            {
                ResultOutput = "",
                ActivityState = new ActivityStateResult()
                {
                    ScreenId = "2b",
                    Error = new Error { Code = 0, Message = "No error" },
                    Success = true,
                    ActivityMetadata = JsonConvert.SerializeObject(this),
                    ActivityType = nameof(PrintAllLabelsActivity)
                }
            };
        }
    }
}