using Newtonsoft.Json;
using WorkflowConfiguration.Activities;
using WorkflowConfiguration.Models;
using WorkflowConfigurator.Activities.ScanSpace;
using WorkflowConfigurator.Models;
using WorkflowConfigurator.Models.Activity;

namespace WorkflowConfigurator.Services.PicaviTranslator
{
    public class ScanScreenTranslator : IPicaviScreenTranslator
    {
        public PicaviResponse BuildExecuteResponse(ActivityStateResult activityStateResult)
        {
            ScanActivity scanActivity = JsonConvert.DeserializeObject<ScanActivity>(activityStateResult.ActivityMetadata);

            PicaviResponse response = new PicaviResponse()
            {
                Success = activityStateResult.Success,
                ScreenId = activityStateResult.ScreenId,
                Error = activityStateResult.Error,
                PageData = BuildExecuteData(scanActivity)
            };

            return response;
        }

        public async Task<PicaviResponse> BuildOnholdResponse(string workflowInstanceId, ActivityStateResult activityStateResult)
        {
            throw new NotImplementedException();
        }

        public PicaviResponse BuildResumeResponse(ActivityStateResult activityStateResult)
        {
            ScanActivity scanActivity = JsonConvert.DeserializeObject<ScanActivity>(activityStateResult.ActivityMetadata);

            PicaviResponse response = new PicaviResponse()
            {
                Success = activityStateResult.Success,
                ScreenId = activityStateResult.ScreenId,
                Error = activityStateResult.Error,
                PageData = BuildResumeData(scanActivity)
            };

            return response;
        }

        private PageData BuildExecuteData(ScanActivity scanActivity)
        {
            PageData pageData = new PageData();

            pageData.Hotkeys = new List<Hotkey> { new Hotkey("1", "Back", "back"), new Hotkey("2", "Logoff", "logoff") };



            List<TextValue> textValues = new List<TextValue>();
            textValues.Add(new TextValue("Icon",
                new List<TextValueValue> {
                    new TextValueValue {
                        Value = scanActivity.GetMessageToShow()
                    }
                }));
            textValues.Add(new TextValue("action", scanActivity.GetActionToShow()));

            pageData.TextValues = textValues;
            pageData.Icon = scanActivity.GetIconToShow();
            pageData.Image = scanActivity.GetImageToShow();


            return pageData;
        }

        private PageData BuildResumeData(ScanActivity scanActivity)
        {
            PageData pageData = new PageData();

            pageData.Hotkeys = new List<Hotkey> { new Hotkey("1", "Back", "back"), new Hotkey("2", "Logoff", "logoff") };


            List<TextValue> textValues = new List<TextValue>();
            textValues.Add(new TextValue("Icon",
                new List<TextValueValue> {
                    new TextValueValue {
                        Value = scanActivity.GetMessageToShow()
                    }
                }));

            textValues.Add(new TextValue("action", scanActivity.GetActionToShow()));

            pageData.TextValues = textValues;
            pageData.Icon = scanActivity.GetIconToShow();

            return pageData;
        }
    }
}
