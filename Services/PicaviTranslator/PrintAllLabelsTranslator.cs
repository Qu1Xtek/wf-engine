using Newtonsoft.Json;
using WorkflowConfiguration.Activities;
using WorkflowConfiguration.Models;
using WorkflowConfigurator.Models;
using WorkflowConfigurator.Models.Activity;
using WorkflowConfigurator.Util;

namespace WorkflowConfigurator.Services.PicaviTranslator
{
    public class PrintAllLabelsTranslator : IPicaviScreenTranslator
    {
        public PicaviResponse BuildExecuteResponse(ActivityStateResult activityStateResult)
        {
            PrintAllLabelsActivity scanActivity = JsonConvert.DeserializeObject<PrintAllLabelsActivity>(activityStateResult.ActivityMetadata);

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
            PrintAllLabelsActivity scanActivity = JsonConvert.DeserializeObject<PrintAllLabelsActivity>(activityStateResult.ActivityMetadata);

            PicaviResponse response = new PicaviResponse()
            {
                Success = activityStateResult.Success,
                ScreenId = activityStateResult.ScreenId,
                Error = activityStateResult.Error,
                PageData = BuildResumeData(scanActivity)
            };

            return response;
        }

        private PageData BuildExecuteData(PrintAllLabelsActivity scanActivity)
        {
            PageData pageData = new PageData();

            pageData.Hotkeys = new List<Hotkey> { new Hotkey("1", "Back", "back"), new Hotkey("2", "Logoff", "logoff") };

            List<TextValue> textValues = new List<TextValue>();
            textValues.Add(new TextValue("Icon",
                new List<TextValueValue> {
                    new TextValueValue {
                        Value = StringConstants.LABELS_WILL_BE_PRINTED
                    }
                }));
            textValues.Add(new TextValue("action", "Confirm"));

            pageData.TextValues = textValues;
            pageData.Icon = "info";
            pageData.Image = "6wellPlate";


            return pageData;
        }

        private PageData BuildResumeData(PrintAllLabelsActivity scanActivity)
        {
            return BuildExecuteData(scanActivity);
        }
    }
}