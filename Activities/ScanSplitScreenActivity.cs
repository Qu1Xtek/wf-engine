using Newtonsoft.Json;
using WorkflowConfiguration.Interface;
using WorkflowConfigurator.Attributes;
using WorkflowConfigurator.Models.Activity;
using WorkflowConfigurator.Services.Materials;
using WorkflowConfigurator.Services.Printer;
using WorkflowConfigurator.Util;

namespace WorkflowConfiguration.Activities
{
    [DefaultDefinition("Activity used for scanning a Material based on configuration")]
    [ActivityTitle("Product scanning (expanded)")]
    public class ScanSplitScreenActivity : Activity, IActivity
    {
        private MaterialService _materialService;
        private PrinterService _printerService;


        public ScanSplitScreenActivity()
        {
            using (var scope = Provider.CreateScope())
            {
                _materialService = scope.ServiceProvider.GetService<MaterialService>();
                _printerService = scope.ServiceProvider.GetService<PrinterService>();
            }
        }

        [JsonProperty]
        private string _currentMessageToShow = string.Empty;
        [JsonProperty]
        private string _currentActionToShow = string.Empty;
        [JsonProperty]
        private string _scanImageToShow = string.Empty;
        [JsonProperty]
        private string _iconToShow = string.Empty;
        
        [DescriptionProp("The result of the previous Calculation activity can be used here by typing '<Y>', where Y is the result")]
        public string UserMessage { get; set; }

        [TypeOverwrite("Dropdown")]
        [DataAddress("MaterialAddress")]
        public string ScanType { get; set; }

        public ActivityResult Execute(WorkflowContext workflowContext)
        {
            AddLastCalculationResultIFPlaceholderExists(workflowContext);
            if (!workflowContext.WfInstance.GlobalVariables.ContainsKey("LabelsPrinted"))
            {
                _printerService.CreateLabelsPdf(workflowContext.WfInstance);
            }

            _currentMessageToShow = UserMessage;
            _currentActionToShow = "Scan";

            var material = _materialService.GetById(ScanType);

            _scanImageToShow = material.Image;
            _iconToShow = "scan";

            var result = new ActivityResult()
            {
                ResultOutput = UserMessage,
                ActivityState = new ActivityStateResult()
                {
                    ScreenId = "3a",
                    Error = new Error { Code = 0, Message = "No error" },
                    Success = true,
                    ActivityMetadata = JsonConvert.SerializeObject(this),
                    ActivityType = nameof(ScanSplitScreenActivity)
                }
            };

            return Pause(result);
        }

        private void AddLastCalculationResultIFPlaceholderExists(WorkflowContext workflowContext)
        {
            if (UserMessage.Contains(StringConstants.CALCULATION_TEXT_PLACEHOLDER) && workflowContext.WfInstance.GlobalVariables.ContainsKey("LastCalculationResult"))
            {
                UserMessage = UserMessage.Replace(StringConstants.CALCULATION_TEXT_PLACEHOLDER, workflowContext.WfInstance.GlobalVariables["LastCalculationResult"].ToString());
            }
        }
        public override ActivityResult Resume(WorkflowContext workflowContext)
        {
            if (!String.IsNullOrEmpty(workflowContext.ProcessAction.ActivityData.Hotkey))
            {
                return HandleHotkey(workflowContext);

            }

            if (workflowContext.ProcessAction.ActivityData?.Confirm == "confirm")
            {
                _currentMessageToShow = "Scan confirmed";

                var resultCOnfirm = new ActivityResult()
                {
                    ResultOutput = "Scan confirmed",
                    ActivityState = new ActivityStateResult()
                    {
                        Error = new Error { Code = 0, Message = "No error" },
                        Success = true,
                        ActivityMetadata = JsonConvert.SerializeObject(this),
                        ActivityType = nameof(ScanSplitScreenActivity),
                        Input = workflowContext.ProcessAction.ActivityData?.Confirm
                    }
                };

                return Done(resultCOnfirm);
            }

            workflowContext.WfInstance.SetLastOutput(workflowContext.ProcessAction.ActivityData.ScannedValue);
            _currentMessageToShow = "Confirm Scan";
            _currentActionToShow = "Confirm";
            _iconToShow = "confirm";

            var result = new ActivityResult()
            {
                ResultOutput = "Scanned data arrived",
                ActivityState = new ActivityStateResult()
                {
                    ScreenId = "3b",
                    Error = new Error { Code = 0, Message = "No error" },
                    Success = true,
                    ActivityMetadata = JsonConvert.SerializeObject(this),
                    ActivityType = nameof(ScanSplitScreenActivity),
                    Input = workflowContext.ProcessAction.ActivityData.ScannedValue
                }
            };


            return Pause(result);
        }

        public string GetMessageToShow()
        {
            return _currentMessageToShow;
        }
        public string GetActionToShow()
        {
            return _currentActionToShow;
        }

        public string GetImageToShow()
        {
            return _scanImageToShow;
        }

        public string GetIconToShow()
        {
            return _iconToShow;
        }
    }
}