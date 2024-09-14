using MongoDB.Bson.Serialization.Conventions;
using Newtonsoft.Json;
using SkiaSharp;
using WorkflowConfiguration.Interface;
using WorkflowConfigurator.Attributes;
using WorkflowConfigurator.Models.Activity;
using WorkflowConfigurator.Models.ActivityAtributeTypes;
using WorkflowConfigurator.Models.Materials;
using WorkflowConfigurator.Models.Workflow;
using WorkflowConfigurator.Repositories;
using WorkflowConfigurator.Services;
using WorkflowConfigurator.Services.Materials;
using WorkflowConfigurator.Services.Printer;
using WorkflowConfigurator.Util;

namespace WorkflowConfigurator.Activities.ScanSpace
{
    [DefaultDefinition("Activity used for scanning a Material based on configuration")]
    [ActivityTitle("Product scanning new")]
    public class ScanActivityNew : Activity, IActivity
    {
        private MaterialService _materialService;
        private PrinterService _printerService;
        public ScanActivityNew()
        {
            using (var scope = Provider.CreateScope())
            {
                _materialService = scope.ServiceProvider.GetService<MaterialService>();
                _printerService = scope.ServiceProvider.GetService<PrinterService>();
            }
        }
        [DescriptionProp("The result of the previous Calculation activity can be used here by typing '<Y>', where Y is the result")]

        public string UserMessage { get; set; }

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

        [TypeOverwrite("Dropdown")]
        [DataAddress("MaterialAddress")]
        public string ScanType { get; set; }

        [TypeOverwrite(OverwrittenType.DROPDOWN)]
        [PredifinedValues("Items, Equipment, Others")]
        public string Category { get; set; }

        public ActivityResult Execute(WorkflowContext workflowContext)
        {
            AddLastCalculationResultIFPlaceholderExists(workflowContext);
            _currentMessageToShow = UserMessage;
            _currentActionToShow = "Scan";

            //Material material = _materialService.GetById(ScanType);

            _scanImageToShow = "Cryotube";// material.Image;

            _iconToShow = "scan";
            _currentState = ScreenStates.SCAN.ToString();

            var result = new ActivityResult()
            {
                ResultOutput = UserMessage,
                ActivityState = new ActivityStateResult()
                {
                    ScreenId = "2a",
                    Error = new Error { Code = 0, Message = "No error" },
                    Success = true,
                    ActivityMetadata = JsonConvert.SerializeObject(this),
                    ActivityType = nameof(ScanActivityNew)
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

            if (!string.IsNullOrEmpty(workflowContext.ProcessAction.ActivityData.Hotkey))
            {
                return HandleHotkey(workflowContext);
            }

            if (workflowContext.ProcessAction.ActivityData?.Confirm == "confirm"
                && _currentState.Equals(ScreenStates.MESSAGE.ToString()))
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
                        ActivityType = nameof(ScanActivityNew),
                        Input = workflowContext.ProcessAction.ActivityData?.Confirm
                    }
                };

                return Done(resultCOnfirm);
            }
            else if (workflowContext.ProcessAction.ActivityData?.Confirm == "confirm"
                && _currentState.Equals(ScreenStates.ERROR_MESSAGE.ToString()))
            {
                _currentMessageToShow = UserMessage;
                _currentActionToShow = "Scan";
                //Material material = _materialService.GetById(ScanType);

                _scanImageToShow = "Cryotube";// material.Image;

                _iconToShow = "scan";
                _currentState = ScreenStates.SCAN.ToString();

                var scanResult = new ActivityResult()
                {
                    ResultOutput = UserMessage,
                    ActivityState = new ActivityStateResult()
                    {
                        ScreenId = "2a",
                        Error = new Error { Code = 0, Message = "No error" },
                        Success = true,
                        ActivityMetadata = JsonConvert.SerializeObject(this),
                        ActivityType = nameof(ScanActivityNew)
                    }
                };

                return Pause(scanResult);
            }

            var scannedValue = workflowContext.ProcessAction.ActivityData.ScannedValue;
            var isInvalidScan = !(IsScannedBarcodeValid(scannedValue, workflowContext).GetAwaiter().GetResult());

            if (isInvalidScan)
            {
                _currentMessageToShow = "You have scanned the wrong barcode, please scan the correct one";
                _currentActionToShow = "Confirm";
                _iconToShow = "confirm";
                _currentState = ScreenStates.ERROR_MESSAGE.ToString();

                var errorMessageResult = new ActivityResult()
                {
                    ResultOutput = "Wrong barcode scanned",
                    ActivityState = new ActivityStateResult()
                    {
                        ScreenId = "2b",
                        Error = new Error { Code = 0, Message = "No error" },
                        Success = true,
                        ActivityMetadata = JsonConvert.SerializeObject(this),
                        ActivityType = nameof(ScanActivityNew),
                        Input = workflowContext.ProcessAction.ActivityData.ScannedValue
                    }
                };


                return Pause(errorMessageResult);
            }

            workflowContext.WfInstance.SetLastOutput(workflowContext.ProcessAction.ActivityData.ScannedValue);
            _currentMessageToShow = "Confirm Scan";
            _currentActionToShow = "Confirm";
            _iconToShow = "confirm";
            _currentState = ScreenStates.MESSAGE.ToString();

            var result = new ActivityResult()
            {
                ResultOutput = "Scanned data arrived",
                ActivityState = new ActivityStateResult()
                {
                    ScreenId = "2b",
                    Error = new Error { Code = 0, Message = "No error" },
                    Success = true,
                    ActivityMetadata = JsonConvert.SerializeObject(this),
                    ActivityType = nameof(ScanActivityNew),
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


        private async Task<bool> IsScannedBarcodeValid(string scannedValue, WorkflowContext context)
        {
            if (string.IsNullOrWhiteSpace(scannedValue))
            {
                return false;
            }

            var result = await _materialService.CheckForInvalidScan(scannedValue, ScanType);

            if (result)
            {
                var activityId = context.WfInstance.CurrentActivityId;
                context.WfInstance.GlobalVariables[$"ScanVar-{activityId}"] = scannedValue;
            }

            return result;
        }
    }
}