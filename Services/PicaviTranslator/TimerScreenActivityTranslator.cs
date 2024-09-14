using Newtonsoft.Json;
using WorkflowConfiguration.Activities;
using WorkflowConfiguration.Interface;
using WorkflowConfiguration.Models;
using WorkflowConfigurator.Models;
using WorkflowConfigurator.Models.Activity;
using WorkflowConfigurator.Repositories;

namespace WorkflowConfigurator.Services.PicaviTranslator
{
    public class TimerScreenActivityTranslator : IPicaviScreenTranslator
    {
        private readonly ActivityTimerRepository _timerService;
        public TimerScreenActivityTranslator(ActivityTimerRepository activityTimerService)
        {
            this._timerService = activityTimerService;
        }
        public PicaviResponse BuildExecuteResponse(ActivityStateResult activityStateResult)
        {
            TimerActivity userConfigurableTimerActivity = 
                JsonConvert.DeserializeObject<TimerActivity>(
                    activityStateResult.ActivityMetadata);

            PicaviResponse response = new PicaviResponse()
            {
                Success = activityStateResult.Success,
                ScreenId = activityStateResult.ScreenId,
                Error = activityStateResult.Error,
                PageData = BuildExecuteData(userConfigurableTimerActivity)
            };

            return response;
        }

        public async Task<PicaviResponse> BuildOnholdResponse(string workflowInstanceId, ActivityStateResult activityStateResult)
        {
            TimerActivity userConfigurableTimerActivity = 
                JsonConvert.DeserializeObject<TimerActivity>(
                    activityStateResult.ActivityMetadata);

            PicaviResponse response = new PicaviResponse()
            {
                Success = activityStateResult.Success,
                ScreenId = activityStateResult.ScreenId,
                Error = activityStateResult.Error,
                PageData = await BuildOnholdData(workflowInstanceId, userConfigurableTimerActivity)
            };

            return response;
        }

        public PicaviResponse BuildResumeResponse(ActivityStateResult activityStateResult)
        {
            return BuildExecuteResponse(activityStateResult);
        }

        private PageData BuildExecuteData(TimerActivity userConfigurableTimerActivity)
        {
            PageData pageData = new PageData();

            List<TextValue> textValues = new List<TextValue>();
            textValues.Add(new TextValue("Title", userConfigurableTimerActivity.UserMessage));
            pageData.Hotkeys = new List<Hotkey> { new Hotkey("1", "Back", "back"), new Hotkey("2", "Logoff", "logoff") };

            pageData.TextValues = textValues;
            pageData.InputVars = new InputVariables() { Size = "medium", DateFormat = "hh:mm:ss" };

            return pageData;
        }

        private async Task<PageData> BuildOnholdData(string workflowInstanceId, TimerActivity userConfigurableTimerActivity)
        {
            PageData pageData = new PageData();

            pageData.Hotkeys = new List<Hotkey> { new Hotkey("1", "Back", "back"), new Hotkey("2", "Logoff", "logoff") };

            ActivityTimer timer = await _timerService.GetByWorkflowInstanceIdAsync(workflowInstanceId);

            List<TextValue> textValues = new List<TextValue>();
            textValues.Add(new TextValue("Icon",
                new List<TextValueValue> {
                    new TextValueValue {
                        Value = "Timer running - Time left : " + $"{timer.ActiveUntil.Subtract(DateTime.UtcNow):hh\\:mm\\:ss}"
                    }
                }));

            pageData.TextValues = textValues;
            pageData.Icon = "work";
            pageData.Image = "incubator";
            pageData.ActionReoccuring = 3000;


            return pageData;
        }
    }


}
