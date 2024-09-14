using WorkflowConfiguration.Models;
using WorkflowConfigurator.Models.Activity;
using WorkflowConfigurator.Models.Authorization;
using WorkflowConfigurator.Models.Workflow;
using WorkflowConfigurator.Repositories;
using WorkflowConfigurator.Services.PicaviTranslator;
using WorkflowConfigurator.Services.Workflow.Execution;
using WorkflowConfigurator.Services.Workflow;
using WorkflowConfigurator.Activities;
using Newtonsoft.Json;
using WorkflowConfiguration.Activities;

namespace WorkflowConfigurator.Services.Service
{
    public class ScreenBuilderService
    {
        private readonly WorkflowInstanceService _workflowInstanceService;
        private readonly WorkflowDefinitionService _workflowDefinitionService;
        private readonly UserService _userService;
        private readonly UserSessionService _userSessionService;
        private readonly WebService _webService;
        private readonly ILogger<WorkflowInvokerService> _logger;

        private static string ERROR_MESSAGE = "An internal error has occurred, please try again later. If the problem persists, please contact your administrator.";
        public ScreenBuilderService(WorkflowInstanceService workflowInstanceService,
            UserSessionService userSessionService,
            WebService webService,
            WorkflowDefinitionService workflowDefinitionService,
            UserService userService,
            ILogger<WorkflowInvokerService> logger
            )

        {
            _workflowInstanceService = workflowInstanceService;
            _userSessionService = userSessionService;
            _webService = webService;
            _workflowDefinitionService = workflowDefinitionService;
            _userService = userService;
            _logger = logger;
        }

        public async Task<PicaviResponse> BuildWorkflowInstanceSelectionScreen()
        {
            int id = 1;
            ActionData newWorkflowInstance = new ActionData { Id = id, Value = "New" };
            List<ActionData> actionData = new List<ActionData>();
            actionData.Add(newWorkflowInstance);

            UserSession userSession = await _userSessionService.GetAsync(_webService.UserLogin.Email);
            List<WorkflowInstance> workflowInstances = await _workflowInstanceService.GetUnfinishedOrderedByStatusAsync(userSession.WorkflowDefinitionId);

            foreach (WorkflowInstance workflowInstance in workflowInstances)
            {
                ActionData intanceActionData = new ActionData { Id = ++id, Value = workflowInstance.InstanceName };
                actionData.Add(intanceActionData);
            }

            PicaviResponse response = new PicaviResponse
            {
                ScreenId = "1",
                Success = true,
                Error = new Error
                {
                    Code = 0,
                    Message = "No error"
                },
                PageData = new PageData
                {
                    Hotkeys = new List<Hotkey> { new Hotkey("1", "Logoff", null), new Hotkey("2", "Back", "back") },
                    TextValues = new List<TextValue> { new TextValue("Title", "Select WorkflowInstance") },
                    ActionData = actionData
                }
            };

            return response;
        }

        public async Task<ActivityStateResult> NewBuildWorkflowInstanceSelectionScreen()
        {

            int id = 1;
            ActionData newWorkflowInstance = new ActionData { Id = id, Value = "New" };
            List<ActionData> actionData = new List<ActionData>();
            actionData.Add(newWorkflowInstance);

            UserSession userSession = await _userSessionService.GetAsync(_webService.UserLogin.Email);
            List<WorkflowInstance> workflowInstances = await _workflowInstanceService.GetUnfinishedOrderedByStatusAsync(userSession.WorkflowDefinitionId);

            foreach (WorkflowInstance workflowInstance in workflowInstances)
            {
                ActionData intanceActionData = new ActionData { Id = ++id, Value = workflowInstance.InstanceName };
                actionData.Add(intanceActionData);
            }

            ListActivity actvity = new ListActivity("Select WorkflowInstance", actionData);

            ActivityStateResult response = new ActivityStateResult
            {
                ScreenId = "1",
                Success = true,
                Error = new Error
                {
                    Code = 0,
                    Message = "No error"
                },
                ActivityType = "ListActivity",
                ActivityMetadata = JsonConvert.SerializeObject(actvity)
            };

            return response;
        }

        public async Task<PicaviResponse> BuildWorkflowDefinitionSelectionScreen()
        {
            int id = 0;
            List<ActionData> actionData = new List<ActionData>();

            List<WorkflowDefinition> workflowDefinitions = await _workflowDefinitionService.GetAsync();           

            foreach (WorkflowDefinition workflowDefinition in workflowDefinitions)
            {
                if (workflowDefinition.CompanyId == _webService.UserLogin.CompanyId) /// Web service should not be used here, but only it's param to be passed
                {
                    ActionData intanceActionData = new ActionData { Id = id++, Value = workflowDefinition.Name };
                    actionData.Add(intanceActionData);
                }
            }

            PicaviResponse response = new PicaviResponse
            {
                ScreenId = "1",
                Success = true,
                Error = new Error
                {
                    Code = 0,
                    Message = "No error"
                },
                PageData = new PageData
                {
                    Hotkeys = new List<Hotkey> { new Hotkey("1", "Logoff", null) },
                    TextValues = new List<TextValue> { new TextValue("Title", "Select WorkflowDefinition") },
                    ActionData = actionData
                }
            };

            return response;
        }

        public async Task<ActivityStateResult> NewBuildWorkflowDefinitionSelectionScreen()
        {
            int id = 0;
            List<ActionData> actionData = new List<ActionData>();

            List<WorkflowDefinition> workflowDefinitions = await _workflowDefinitionService.GetAsync();

            foreach (WorkflowDefinition workflowDefinition in workflowDefinitions)
            {
                if (workflowDefinition.CompanyId == _webService.UserLogin.CompanyId) /// Web service should not be used here, but only it's param to be passed
                {
                    ActionData intanceActionData = new ActionData { Id = id++, Value = workflowDefinition.Name };
                    actionData.Add(intanceActionData);
                }
            }

            ListActivity activity = new ListActivity("Select WorkflowDefinition", actionData);
            ActivityStateResult response = new ActivityStateResult
            {
                ScreenId = "1",
                Success = true,
                Error = new Error
                {
                    Code = 0,
                    Message = "No error"
                },
                ActivityType = "ListActivity",
                ActivityMetadata = JsonConvert.SerializeObject(activity)
            };

            return response;
        }
        public PicaviResponse BuildErrorResponseScreen(Exception ex)
        {
            _logger.LogError(ex, ex.Message);

            Error error = new Error() { Code = 500, Message = ex.Message };
            PicaviResponse response = new PicaviResponse()
            {
                Success = false,
                ScreenId = "2b",
                Error = error,
                PageData = new PageData
                {
                    Hotkeys = new List<Hotkey> { new Hotkey("1", "Back", "back"), new Hotkey("2", "Logoff", "logoff") },
                    TextValues = new List<TextValue> { new TextValue("Title", ERROR_MESSAGE), new TextValue("action", "Confirm") },
                    Icon = "error",
                    Image = "error"
                }
            };
            return response;
        }

        public ActivityStateResult NewBuildErrorResponseScreen(Exception ex)
        {
            _logger.LogError(ex, ex.Message);

            Error error = new Error() { Code = 500, Message = ex.Message };

            MessageScreenActivity activity = new MessageScreenActivity()
            {
                UserMessage = ERROR_MESSAGE
            };

            ActivityStateResult response = new ActivityStateResult()
            {
                Success = false,
                ScreenId = "2b",
                Error = error,
                ActivityType = "MessageScreenActivity",
                ActivityMetadata = JsonConvert.SerializeObject(activity)
            };

            return response;
        }
    }
}
