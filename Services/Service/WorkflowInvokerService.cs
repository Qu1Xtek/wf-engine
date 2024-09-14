using WorkflowConfiguration.Activities;
using WorkflowConfiguration.Interface;
using WorkflowConfiguration.Models;
using WorkflowConfigurator.Models;
using WorkflowConfigurator.Models.Activity;
using WorkflowConfigurator.Models.Authorization;
using WorkflowConfigurator.Models.DTO.Workflow;
using WorkflowConfigurator.Models.Workflow;
using WorkflowConfigurator.Repositories;
using WorkflowConfigurator.Services.PicaviTranslator;
using WorkflowConfigurator.Services.Workflow;
using WorkflowConfigurator.Services.Workflow.Execution;

namespace WorkflowConfigurator.Services.Service
{
    public class WorkflowInvokerService
    {
        private readonly WorkflowInstanceService _workflowInstanceService;
        private readonly WorkflowDefinitionService _workflowDefinitionService;
        private readonly UserService _userService;
        private readonly UserSessionService _userSessionService;
        private readonly WebService _webService;
        private readonly ScreenTranslateService _screenTranslateService;
        private readonly InstanceMaintainer _instanceConfigurator;
        private readonly HotkeyHandler _hotkeyHandler;
        private WorkflowMaster _wfRunner;
        private readonly ILogger<WorkflowInvokerService> _logger;
        private readonly ScreenBuilderService _screenBuilderService;

        private static string NEW = "New";
        private static string ERROR_MESSAGE = "An internal error has occurred, please try again later. If the problem persists, please contact your administrator.";
        public WorkflowInvokerService(WorkflowInstanceService workflowInstanceService,
            ScreenTranslateService screenTranslateService,
            UserSessionService userSessionService,
            WebService webService,
            WorkflowDefinitionService workflowDefinitionService,
            UserService userService,
            InstanceMaintainer instanceMaintainer,
            WorkflowMaster wfRunner,
            HotkeyHandler hotkeyHandler,
            ILogger<WorkflowInvokerService> logger,
            ScreenBuilderService screenBuilderService
            )

        {
            _workflowInstanceService = workflowInstanceService;
            _screenTranslateService = screenTranslateService;
            _userSessionService = userSessionService;
            _webService = webService;
            _workflowDefinitionService = workflowDefinitionService;
            _userService = userService;
            _instanceConfigurator = instanceMaintainer;
            _wfRunner = wfRunner;
            _hotkeyHandler = hotkeyHandler;
            _logger = logger;
            _screenBuilderService = screenBuilderService;
        }


        public async Task<ServiceResponse> ProgressWorkflow(ExecuteProcessAction executeProcessAction)
        {
            try
            {
                UserSession userSession = await _userSessionService.GetAsync(_webService.UserLogin.Email);
                string workflowInstanceId = userSession.WorkflowInstanceId;
                string workflowDefinitionId = userSession.WorkflowDefinitionId;
                executeProcessAction.UserEmail = _webService.UserLogin.Email;

                if (!String.IsNullOrEmpty(executeProcessAction?.ActivityData?.Hotkey))
                {
                    _hotkeyHandler.HandleHotkey(new WorkflowContext(null, null, new WorkflowInstance(), executeProcessAction));

                    return new ServiceResponse
                    {
                        ErrorCode = 200
                    };
                }

                if (!String.IsNullOrEmpty(workflowInstanceId))
                {
                    ActivityStateResult response = await _wfRunner.ProgressWorkflow(executeProcessAction, workflowInstanceId);

                    return new ServiceResponse
                    {
                        ResponseObject = response
                    };
                }
                else if (String.IsNullOrEmpty(workflowDefinitionId))
                {
                    if (string.IsNullOrWhiteSpace(executeProcessAction.ActivityData.ActionData))
                    {
                        return new ServiceResponse
                        {
                            ErrorCode = 400
                        };
                    }

                    WorkflowDefinition workflowDefinition = await _workflowDefinitionService.GetByNameAsync(executeProcessAction.ActivityData.ActionData);

                    if (workflowDefinition == null)
                    {
                        return new ServiceResponse
                        {
                            ErrorCode = 400,
                            Message = $"Workflow definition with name ({executeProcessAction.ActivityData.ActionData}) cannot be found"
                        };
                    }

                    userSession.WorkflowDefinitionId = workflowDefinition.Id;

                    await _userSessionService.UpdateAsync(userSession.Id, userSession);

                    return new ServiceResponse
                    {
                        ErrorCode = 200
                    };
                }
                else
                {
                    string? incomingWorkflowInstanceName = executeProcessAction?.ActivityData?.ActionData;

                    if (incomingWorkflowInstanceName == NEW)
                    {
                        WorkflowInstance instance = await _instanceConfigurator
                            .CreateInstanceAsync(workflowDefinitionId, _webService.UserLogin.Email);

                        var response = await _wfRunner.ProgressWorkflow(
                            new ExecuteProcessAction() { UserEmail = _webService.UserLogin.Email },
                            instance.Id);

                        userSession.WorkflowInstanceId = instance.Id;


                    }
                    else
                    {
                        WorkflowInstance instance = await _workflowInstanceService.GetByNameAsync(incomingWorkflowInstanceName);
                        userSession.WorkflowInstanceId = instance.Id;
                    }

                    await _userSessionService.UpdateAsync(userSession.Id, userSession);

                    return new ServiceResponse
                    {
                        ErrorCode = 200
                    };
                }
            }
            catch (Exception ex )
            {
                _logger.LogError(ex, ex.Message);
                return new ServiceResponse
                {
                    ResponseObject = ERROR_MESSAGE,
                    ErrorCode = 500,
                    Message = ERROR_MESSAGE
                };
            }
            
        }

        public async Task<PicaviResponse> GetState()
        {
            UserSession session = await _userSessionService.GetAsync(_webService.UserLogin.Email);
            try
            {
                return await BuildResponse(session);

            }
            catch (Exception ex)
            {
                var response = _screenBuilderService.BuildErrorResponseScreen(ex);
                return response;
            }
        }

        public async Task<ActivityStateResult> NewGetState()
        {
            UserSession session = await _userSessionService.GetAsync(_webService.UserLogin.Email);
            try
            {
                return await NewBuildResponse(session);

            }
            catch (Exception ex)
            {
                var response = _screenBuilderService.NewBuildErrorResponseScreen(ex);
                return response;
            }
        }

        public async Task<string> InjectScan(RunningWFScan data)
        {
            return (await _wfRunner.InjectScannedValue(data)).Message;
        }

        private async Task<PicaviResponse> BuildResponse(UserSession userSession)
        {
            if (!String.IsNullOrEmpty(userSession.WorkflowInstanceId))
            {
                WorkflowInstance workflowInstance = await _workflowInstanceService.GetByIdAsync(userSession.WorkflowInstanceId);

                if (workflowInstance.Status == WorkflowStatus.AWAIT_RESUME || workflowInstance.Status == WorkflowStatus.FINAL_ACTIVITY)
                {
                    return _screenTranslateService.TranslateResumeData(workflowInstance.CurrentState);
                }
                else if (workflowInstance.Status == WorkflowStatus.ONHOLD)
                {
                    return await _screenTranslateService.TranslateOnholdData(workflowInstance.Id, workflowInstance.CurrentState);
                }

                return _screenTranslateService.TranslateExecuteData(workflowInstance.CurrentState);
            }
            else if (!String.IsNullOrEmpty(userSession.WorkflowDefinitionId))
            {
                return await _screenBuilderService.BuildWorkflowInstanceSelectionScreen();
            }
            else
            {
                return await _screenBuilderService.BuildWorkflowDefinitionSelectionScreen();
            }
        }
        private async Task<ActivityStateResult> NewBuildResponse(UserSession userSession)
        {
            if (!String.IsNullOrEmpty(userSession.WorkflowInstanceId))
            {
                WorkflowInstance workflowInstance = await _workflowInstanceService.GetByIdAsync(userSession.WorkflowInstanceId);
                
                return workflowInstance.CurrentState;
            }
            else if (!String.IsNullOrEmpty(userSession.WorkflowDefinitionId))
            {
                return await _screenBuilderService.NewBuildWorkflowInstanceSelectionScreen();
            }
            else
            {
                return await _screenBuilderService.NewBuildWorkflowDefinitionSelectionScreen();
            }
        }
    }
}
