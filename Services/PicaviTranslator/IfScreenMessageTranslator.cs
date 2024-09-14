using Newtonsoft.Json;
using WorkflowConfiguration.Activities;
using WorkflowConfiguration.Models;
using WorkflowConfigurator.Models;
using WorkflowConfigurator.Models.Activity;

namespace WorkflowConfigurator.Services.PicaviTranslator
{
    public class IfScreenMessageTranslator : IPicaviScreenTranslator
    {
        public PicaviResponse BuildExecuteResponse(ActivityStateResult activityStateResult)
        {
            IfScreenMessageActivity ifScreenMessageActivity = JsonConvert.DeserializeObject<IfScreenMessageActivity>(activityStateResult.ActivityMetadata);

            PicaviResponse response = new PicaviResponse()
            {
                Success = activityStateResult.Success,
                ScreenId = activityStateResult.ScreenId,
                Error = activityStateResult.Error,
                PageData = BuildExecuteData(ifScreenMessageActivity)
            };

            return response;
        }

        public async Task<PicaviResponse> BuildOnholdResponse(string workflowInstanceId, ActivityStateResult activityStateResult)
        {
            throw new NotImplementedException();
        }

        public PicaviResponse BuildResumeResponse(ActivityStateResult activityStateResult)
        {
            return BuildExecuteResponse(activityStateResult);
        }


        private PageData BuildExecuteData(IfScreenMessageActivity ifScreenMessageActivity)
        {
            PageData pageData = new PageData();

            pageData.Hotkeys = new List<Hotkey> { new Hotkey("1", "Back", "back"), new Hotkey("2", "Logoff", "logoff") };

            List<TextValue> textValues = new List<TextValue>();
            textValues.Add(new TextValue("Title", ifScreenMessageActivity.UserMessage));
            textValues.Add(new TextValue("Question",
                new List<TextValueValue> {
                    new TextValueValue {
                        Label = "Option1",
                        Value = "True"
                    },
                    new TextValueValue {
                        Label = "Option2",
                        Value = "False"
                    }
                }));

            pageData.TextValues = textValues;

            return pageData;
        }
    }
}
