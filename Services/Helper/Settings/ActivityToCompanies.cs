using WorkflowConfiguration.Activities;
using WorkflowConfigurator.Activities;
using WorkflowConfigurator.Activities.ScanSpace;

namespace WorkflowConfigurator.Services.Helper.Settings
{
    public class ActivityToCompanies
    {
        public static Dictionary<string, List<string>> Mapping { get; } =
            new Dictionary<string, List<string>>()
            {
                { "conf", new List<string> 
                    {
                        nameof(MessageScreenActivity),
                        nameof(ScanActivity),
                        nameof(ScanSplitScreenActivity),
                        nameof(IfScreenLastOutcome),
                        nameof(IfScreenMessageActivity),
                        nameof(InputScreenActivity),
                        //nameof(InputSplitScreenActivity),
                        //nameof(CalculationActivity),
                        //nameof(InputWithAddonsScreenActivity),
                        //nameof(MiniWFActivity),
                        nameof(PrintAllLabelsActivity),
                        nameof(TimerActivity),
                        nameof(UserConfigurableTimerActivity),
                        nameof(MeasurementActivity),
                    }
                },
            };
    }
}
