using Newtonsoft.Json;
using WorkflowConfiguration.Activities;
using WorkflowConfiguration.Models;
using WorkflowConfigurator.Models;
using WorkflowConfigurator.Models.Activity;

namespace WorkflowConfigurator.Services.PicaviTranslator
{
    public class InputScreenTranslator : IPicaviScreenTranslator
    {
        public PicaviResponse BuildExecuteResponse(ActivityStateResult activityStateResult)
        {
            InputScreenActivity inputScreenActivity = JsonConvert.DeserializeObject<InputScreenActivity>(activityStateResult.ActivityMetadata);

            PicaviResponse response = new PicaviResponse()
            {
                Success = activityStateResult.Success,
                ScreenId = activityStateResult.ScreenId,
                Error = activityStateResult.Error,
                PageData = BuildExecuteData(inputScreenActivity)
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

        private PageData BuildExecuteData(InputScreenActivity inputScreenActivity)
        {
            PageData pageData = new PageData();

            pageData.Hotkeys = new List<Hotkey> { new Hotkey("1", "Back", "back"), new Hotkey("2", "Logoff", "logoff") };



            List<TextValue> textValues = new List<TextValue>();
            textValues.Add(new TextValue("Title", inputScreenActivity.UserMessage));
            textValues.Add(new TextValue("Icon",
                new List<TextValueValue> {
                    new TextValueValue {
                        Value = "Input"
                    }
                }));
            textValues.Add(new TextValue("action", "Confirm"));
            //textValues.Add(new TextValue("Table",
            //    new List<TextValueValue> {
            //        new TextValueValue {
            //            Label = "label1",
            //            Value = "some text"
            //        },
            //        new TextValueValue {
            //            Label = "label2",
            //            Value = "some text"
            //        }
            //    }));

            pageData.TextValues = textValues;
            pageData.InputVars = new InputVariables() { Lenght = 4, Chars = "numeric", Size = "medium" };
            pageData.Icon = "count";


            return pageData;
        }
    }


}
