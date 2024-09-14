using WorkflowConfiguration.Activities;
using WorkflowConfigurator.Activities.ScanSpace;
using WorkflowConfigurator.Models.Activity;

namespace WorkflowConfigurator.Services.PicaviTranslator
{
    public class ScreenTranslateService
    {

        private readonly ScanScreenTranslator _scanScreenTranslator;
        private readonly ScanSplitScreenTranslator _scanSplitScreenTranslator;
        private readonly InputSplitScreenTranslator _inputSplitScreenTranslator;
        private readonly InputScreenTranslator _inputScreenTranslator;
        private readonly InputWithAddonsScreenTranslator _inputWithAddonsScreenTranslator;
        private readonly MessageScreenTranslator _messageScreenTranslator;
        private readonly PrintAllLabelsTranslator _printAllLabelsTranslator;
        private readonly TimerScreenActivityTranslator _timerScreenActivity;
        private readonly IfScreenMessageTranslator _ifScreenMessageTranslator;
        private readonly UserConfigurableTimerTranslator _userConfigurableTimerTranslator;

        public Dictionary<string, IPicaviScreenTranslator> _translators = new Dictionary<string, IPicaviScreenTranslator>();

        public ScreenTranslateService(
            ScanScreenTranslator scanScreenTranslator,
            ScanSplitScreenTranslator scanSplitScreenTranslator,
            InputSplitScreenTranslator inputSplitScreenTranslator,
            InputScreenTranslator inputScreenTranslator,
            InputWithAddonsScreenTranslator inputWithAddonsScreenTranslator,
            MessageScreenTranslator messageScreenTranslator,
            IfScreenMessageTranslator ifScreenMessageTranslator,
            UserConfigurableTimerTranslator userConfigurableTimerTranslator,
            TimerScreenActivityTranslator timerScreenActivityTranslator,
            PrintAllLabelsTranslator printAllLabelsTranslator)
        {
            _scanScreenTranslator = scanScreenTranslator;
            _scanSplitScreenTranslator = scanSplitScreenTranslator;
            _inputSplitScreenTranslator = inputSplitScreenTranslator;
            _inputScreenTranslator = inputScreenTranslator;
            _inputWithAddonsScreenTranslator = inputWithAddonsScreenTranslator;
            _messageScreenTranslator = messageScreenTranslator;
            _ifScreenMessageTranslator = ifScreenMessageTranslator;
            _userConfigurableTimerTranslator = userConfigurableTimerTranslator;
            _timerScreenActivity = timerScreenActivityTranslator;
            _printAllLabelsTranslator = printAllLabelsTranslator;
            MapTranslators();
        }

        public PicaviResponse TranslateExecuteData(ActivityStateResult activityState)
        {
            return _translators[activityState.ActivityType].BuildExecuteResponse(activityState);
        }

        public PicaviResponse TranslateResumeData(ActivityStateResult activityState)
        {
            return _translators[activityState.ActivityType].BuildResumeResponse(activityState);
        }

        public async Task<PicaviResponse> TranslateOnholdData(string workflowInstanceId, ActivityStateResult activityState)
        {
            return await _translators[activityState.ActivityType].BuildOnholdResponse(workflowInstanceId, activityState);
        }


        private void MapTranslators()
        {
            _translators.Add(nameof(ScanActivity), _scanScreenTranslator);
            _translators.Add(nameof(ScanSplitScreenActivity), _scanSplitScreenTranslator);
            _translators.Add(nameof(InputScreenActivity), _inputScreenTranslator);
            _translators.Add(nameof(InputSplitScreenActivity), _inputSplitScreenTranslator);
            _translators.Add(nameof(InputWithAddonsScreenActivity), _inputWithAddonsScreenTranslator);
            _translators.Add(nameof(MessageScreenActivity), _messageScreenTranslator);
            _translators.Add(nameof(TimerActivity), _timerScreenActivity);
            _translators.Add(nameof(IfScreenMessageActivity), _ifScreenMessageTranslator);
            _translators.Add(nameof(UserConfigurableTimerActivity), _userConfigurableTimerTranslator);
            _translators.Add(nameof(PrintAllLabelsActivity), _printAllLabelsTranslator);
            _translators.Add(nameof(ScanActivityNew), _scanScreenTranslator);
            _translators.Add(nameof(LinkedScanCheckActivity), _scanScreenTranslator);

        }
    }
}