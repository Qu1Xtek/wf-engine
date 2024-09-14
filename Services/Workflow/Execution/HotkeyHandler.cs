using WorkflowConfiguration.Interface;
using WorkflowConfigurator.Models.Activity;
using WorkflowConfigurator.Models.Authorization;
using WorkflowConfigurator.Repositories;

namespace WorkflowConfigurator.Services.Workflow.Execution
{
    public class HotkeyHandler
    {
        private readonly UserSessionService _userSessionService;
        private readonly WebService _webService;

        public static IServiceProvider Provider { get; set; }

        public HotkeyHandler(UserSessionService userSessionService, WebService webService)
        {
            _userSessionService = userSessionService;
            _webService = webService;
        }

        private ActivityResult GoBack(WorkflowContext context, bool toLastScreen = false)
        {
            List<ActivityResult> instanceResults = context.WfInstance.ActivityResults;

            ActivityResult result;

            if (toLastScreen)
            {
                result = instanceResults[instanceResults.Count - 2];
                result.ResultOutput = "Back button used";


                return result;
            }
            else
            {
                result = instanceResults.Count > 0 ? instanceResults.Last() : new ActivityResult();
                result.ResultOutput = "Back button used";

                UserSession session = _userSessionService.GetByUser(context.ProcessAction.UserEmail);

                session.WorkflowInstanceId = "";
                session.WorkflowDefinitionId = "";

                _userSessionService.Update(session.Id, session);

                return result;
            }
        }

        private ActivityResult Logoff(WorkflowContext context)
        {
            _webService.LogoffAndRemoveSession();

            var result = new ActivityResult();

            List<ActivityResult> instanceResults = context.WfInstance.ActivityResults;

            result = instanceResults.Count > 0 ? instanceResults.Last() : result;
            result.ResultOutput = "Logoff button used";

            UserSession session = _userSessionService.GetByUser(context.ProcessAction.UserEmail);

            session.WorkflowInstanceId = "";
            session.WorkflowDefinitionId = "";

            _userSessionService.Update(session.Id, session);

            return result;
        }


        public ActivityResult HandleHotkey(WorkflowContext context, bool toLastScreen = false)
        {
            switch (context.ProcessAction.ActivityData.Hotkey)
            {
                case "Back":
                    return GoBack(context, toLastScreen);
                case "Logoff":
                    return Logoff(context);
                default:
                    return new ActivityResult();

            }
        }
    }
}
