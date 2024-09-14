using WorkflowConfiguration.Models;
using WorkflowConfigurator.Models;
using WorkflowConfigurator.Models.Activity;
using WorkflowConfigurator.Models.DTO.Workflow;
using WorkflowConfigurator.Models.Workflow;
using WorkflowConfigurator.Repositories;
using WorkflowConfigurator.Services.DIP;
using WorkflowConfigurator.Services.Helper.Workflow;

namespace WorkflowConfigurator.Services.Workflow.Execution
{
    public class WorkflowMaster
    {
        private readonly IActivityService _activityService;
        private readonly WorkflowDefinitionRepository _workflowDefinitionService;
        private readonly WorkflowInstanceRepository _workflowInstanceService;
        private readonly ILogger<WorkflowMaster> _logger;
        private readonly DIPService _dipService;
        private readonly MiniWorkflowService _miniWfService;
        private readonly InstanceMaintainer _instanceMaintainer;
        private readonly UserSessionService _userSessionService;
        
        private readonly PathTaker _pathTaker;

        public WorkflowMaster(WorkflowDefinitionRepository workflowDefinitionService,
            WorkflowInstanceRepository workflowInstanceService,
            ILogger<WorkflowMaster> logger,
            DIPService dipService,
            MiniWorkflowService miniWfService,
            InstanceMaintainer instanceMaintainer,
            PathTaker pathTaker, UserSessionService sessionSErvie, ActivityService activityService)
        {
            _workflowDefinitionService = workflowDefinitionService;
            _workflowInstanceService = workflowInstanceService;
            _logger = logger;
            _dipService = dipService;
            _miniWfService = miniWfService;
            _instanceMaintainer = instanceMaintainer;
            _pathTaker = pathTaker;
            _userSessionService = sessionSErvie;
            _activityService = activityService;
        }

        private async Task<ActivityStateResult> CheckDefOnBlockchain(WorkflowDefinition wfDefinition) 
        {
            try
            {
                await _dipService.CheckDefinitionHash(wfDefinition);
                return new ActivityStateResult();
            }
            catch (Exception ex)
            {
                return new ActivityStateResult()
                {
                    Success = false,
                    Error = new Error()
                    {
                        Code = 1,
                        Message = ex.Message
                    }
                };
            }
        }

        public async Task<ActivityStateResult> ProgressWorkflow(ExecuteProcessAction executeActions,
            WorkflowInstance wfInstance,
            WorkflowDefinition wfDefinition)
        {
            var status = WorkflowStatus.RUNNING;
            var progressResponse = new ActivityStateResult();

            while (status == WorkflowStatus.RUNNING)
            {
                //var blockchainCheck = await CheckDefOnBlockchain(wfDefinition);
                //if (blockchainCheck.Error != null)
                //{
                //    return blockchainCheck;
                //}

                var wfData = wfDefinition.WorkflowDefinitionVersionData[wfInstance.DefinitionVer];

                var activity = _pathTaker
                    .GetActivityToExecute(wfData, wfInstance.CurrentActivityId);

                var miniDef = await _pathTaker
                    .CheckForMiniWF(wfData, wfInstance, wfInstance.CurrentActivityId);

                if (miniDef.IsSuccesful)
                {
                    // ENTER MINI WORKFLOW
                    wfData = miniDef.Data;

                    // Assign mini step and reprocess workflow
                    if (wfInstance.MiniWFStep == -1)
                    {
                        wfInstance.MiniWFStep = miniDef.Data.StartingActivity;
                        continue;
                    }

                    activity = _pathTaker
                    .GetActivityToExecute(wfData, wfInstance.MiniWFStep);
                }

                var activityResult = new ActivityResult();
                try
                {
                    activityResult = await new ActivityProcessor(null, null, _activityService).ProcessActivity(
                        wfInstance,
                        activity,
                        executeActions);
                } catch (Exception ex)
                {
                    _logger.LogInformation(ex, ex.Message, null);
                    return await HandleFault(activityResult, executeActions, wfInstance);
                }


                var pathResult = _pathTaker.ProceedToNextStep(wfInstance,
                    wfData,
                    activityResult);

                if (pathResult.MiniWfId == -2)
                {
                    // EXIT mini workflow
                    wfInstance.MiniWFStep = -1;
                    wfData = wfDefinition.WorkflowDefinitionVersionData[wfInstance.DefinitionVer];
                    var exitPath = _pathTaker.ProceedToNextStep(wfInstance,
                        wfData,
                        new ActivityResult { Outcome = Outcomes.DONE.ToString() });

                    pathResult.Outcome = exitPath.Outcome;
                    pathResult.NextActivityId = exitPath.NextActivityId;
                }

                if (wfInstance.MiniWFStep != -1)
                {
                    wfInstance.MiniWFStep = pathResult.MiniWfId;
                }

                // Populate activity result
                activityResult.ApplyData(executeActions.UserEmail, DateTime.UtcNow,
                    wfInstance.CurrentActivityId, pathResult.Outcome);

                wfInstance.ActivityResults.Add(activityResult);

                // Update workflow state (write result and increase step/activityId if needed)
                await _instanceMaintainer.UpdateState(wfInstance,
                    ResultToWFStatus.MapToWFStatus(activityResult.Outcome),
                    activityResult.ActivityState,
                    pathResult.NextActivityId,
                    executeActions);

                status = wfInstance.Status;
                progressResponse = activityResult.ActivityState;
            }

            return progressResponse;
        }

        public async Task<ActivityStateResult> ProgressWorkflow(ExecuteProcessAction executeProcessAction, string workflowInstanceName)
        {
            WorkflowInstance workflowInstance = await _workflowInstanceService.GetAsync(workflowInstanceName);


            if (workflowInstance.Status != WorkflowStatus.ONHOLD)
            {
                return await ProcessWorkflow(workflowInstance, executeProcessAction, workflowInstanceName);
            }
            else
            {
                if (executeProcessAction.ActivityData.Hotkey == "Back")
                {
                    await _instanceMaintainer.RemoveWFSession(executeProcessAction.UserEmail);

                    return null;
                }

                throw new InvalidOperationException($"Invalid action for the workflow instance, progress workflow should not be called when status is {workflowInstance.Status}");
            }
        }

        private async Task<ActivityStateResult> ProcessWorkflow(WorkflowInstance wfInstance, ExecuteProcessAction executeProcessAction, string workflowInstanceId)
        {
            WorkflowDefinition workflowDefinition = await _workflowDefinitionService.GetAsync(wfInstance.WorkflowDefinitionId);
            return await ProgressWorkflow(executeProcessAction, wfInstance, workflowDefinition);
        }

        public async Task<ActivityStateResult> UnholdWorkflow(
            ExecuteProcessAction executeProcessAction,
            string workflowInstanceId)
        {
            try
            {
                WorkflowInstance workflowInstance = await _workflowInstanceService.GetAsync(workflowInstanceId);

                if (workflowInstance.Status == WorkflowStatus.ONHOLD)
                {
                    return await ProcessWorkflow(workflowInstance, executeProcessAction, workflowInstanceId);
                }
                else
                {
                    throw new InvalidOperationException($"Invalid action for the workflow instance unhold workfow should be called only when status is OnHold");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error {ex.Message}, " +
                    $"{nameof(UnholdWorkflow)} with params wfId: {workflowInstanceId} and " +
                    $"action {executeProcessAction?.ActivityData?.ActionData}");

                throw ex;
            }

        }

        public async Task<GRes<bool>> InjectScannedValue(RunningWFScan scanData)
        {
            var session = await _userSessionService.GetAsyncWithNoCreate(scanData.Email);

            if (session == null)
            {
                return GRes<bool>.Fault(false, "User doesn't exist", 1);
            }
            if (string.IsNullOrWhiteSpace(session.WorkflowInstanceId))
            {
                return GRes<bool>.Fault(false, "User has no running instance selected", 1);
            }

            var wfInstance = await _workflowInstanceService.GetAsync(session.WorkflowInstanceId);


            if (wfInstance == null)
            {
                return GRes<bool>.Fault(false, $"User wfInstance with this instanceId {session.WorkflowInstanceId} was not found", 1);
            }

            if (wfInstance.GlobalVariables == null)
            {
                wfInstance.GlobalVariables = new Dictionary<string, string>();
            }

            wfInstance.GlobalVariables["LastScanInput"] = scanData.ScanValue;

            var result = await _instanceMaintainer.AddScanToState(wfInstance);

            var response = GRes<bool>.OK(true, result);

            return response;
        }

        private async Task<ActivityStateResult> HandleFault(ActivityResult activityResult, ExecuteProcessAction executeActions, WorkflowInstance wfInstance)
        {
            ActivityStateResult activityStateResult = new ActivityStateResult();
            activityStateResult.ScreenId = "2b";
            activityStateResult.ActivityMetadata = "{\"UserMessage\": \"An error occured!\"}";
            activityStateResult.ActivityType = "MessageScreenActivity";
            activityStateResult.Error = new Error();

            activityResult.ApplyData(executeActions.UserEmail, DateTime.UtcNow,
                wfInstance.CurrentActivityId, Outcomes.FAULT.ToString());
            activityResult.ActivityState = activityStateResult;

            wfInstance.ActivityResults.Add(activityResult);

            // Update workflow state (write result and increase step/activityId if needed)
            await _instanceMaintainer.UpdateState(wfInstance,
                ResultToWFStatus.MapToWFStatus(activityResult.Outcome),
                activityResult.ActivityState,
                wfInstance.CurrentActivityId,
                executeActions);

            return activityStateResult;
        }
    }
}
