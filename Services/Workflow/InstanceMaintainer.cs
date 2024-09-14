using WorkflowConfiguration.Models;
using WorkflowConfigurator.Models.Activity;
using WorkflowConfigurator.Models.Authorization;
using WorkflowConfigurator.Models.Workflow;
using WorkflowConfigurator.Services.DIP;
using WorkflowConfigurator.Repositories;
using WorkflowConfigurator.Services.Service;

namespace WorkflowConfigurator.Services.Workflow
{
    public class InstanceMaintainer
    {
        private readonly WorkflowDefinitionRepository _workflowDefinitionService;
        private readonly WorkflowInstanceService _workflowInstanceService;
        private readonly DIPService _dipService;
        private readonly UserSessionService _userSessionService;

        public InstanceMaintainer(WorkflowDefinitionRepository workflowDefinitionService,
            WorkflowInstanceService workflowInstanceService,
            DIPService dipService,
            UserSessionService userSessionService)
        {
            _workflowDefinitionService = workflowDefinitionService;
            _workflowInstanceService = workflowInstanceService;
            _dipService = dipService;
            _userSessionService = userSessionService;
        }

        public async Task<WorkflowInstance> CreateInstanceAsync(string workflowDefinitionId, string userName)
        {
            WorkflowDefinition workflowDefinition = await _workflowDefinitionService
                   .GetAsync(workflowDefinitionId);

            WorkflowDefinitionData workflowDefinitionData = workflowDefinition
            .WorkflowDefinitionVersionData[workflowDefinition.Version];

            var workflowInstanceName = _workflowInstanceService.GenerateInstanceName(userName, workflowDefinition.Name);
            WorkflowInstance workflowInstance = new WorkflowInstance()
            {
                InstanceName = workflowInstanceName,
                WorkflowDefinitionId = workflowDefinitionId,
                CurrentActivityId = workflowDefinitionData.StartingActivity,
                CreatedOn = DateTime.UtcNow,
                Owner = userName,
                ActivityConnections = workflowDefinitionData.ActivityConnections,
                DefinitionVer = workflowDefinition.Version,
                CompanyId = workflowDefinition.CompanyId
            };

            await _workflowInstanceService.Create(workflowInstance);


            return workflowInstance;
        }

        private ActivityStateResult SetFinalActivityState()
        {
            var result = new ActivityStateResult();

            result.ScreenId = "2b";
            result.ActivityMetadata = "{\"UserMessage\": \"You have finished the workflow\"}";
            result.ActivityType = "MessageScreenActivity";
            result.Error = new Error();

            return result;
        }

        public async Task UpdateState(WorkflowInstance wfInstance,
            WorkflowStatus newStatus,
            ActivityStateResult activityExecuteResponse,
            int nextStepActivityId,
            ExecuteProcessAction actions)
        {
            try
            {
                wfInstance.CurrentActivityId = nextStepActivityId;
                wfInstance.Status = newStatus;

                wfInstance.CurrentState = newStatus == WorkflowStatus.FINAL_ACTIVITY 
                    ? SetFinalActivityState() 
                    : activityExecuteResponse;

                await _workflowInstanceService.UpdateAsync(wfInstance.Id, wfInstance);


                // Clear session, letting the user select new workflow
                if (newStatus == WorkflowStatus.FINISHED || newStatus == WorkflowStatus.FAULT)
                {
                    await RemoveWFSession(actions.UserEmail);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error {ex}, " +
                    $"{nameof(UpdateState)} crashed with {ex.Message}, with params wfId: {wfInstance.Id} and " +
                    $"response is {activityExecuteResponse.ScreenId}");

                throw ex;
            }
        }

        public async Task<string> AddScanToState(WorkflowInstance wfInstance)
        {
            try
            {
                await _workflowInstanceService.UpdateAsync(wfInstance.Id, wfInstance);
                return $"Succesfully added scan {wfInstance.GlobalVariables["LastScanInput"]} to wf";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error {ex}, " +
                    $"{nameof(AddScanToState)} crashed with {ex.Message}, with params wfId: {wfInstance.Id}");

                return $"Error while addding {wfInstance.GlobalVariables["LastScanInput"]} to wf";
            }
        }

        /// <summary>
        /// Clears the current workflow instance from the user session
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task RemoveWFSession(string email)
        {
            UserSession userSession = await _userSessionService.GetAsync(email);
            await _userSessionService.ResetAsync(userSession.Id, userSession);
        }


    }
}
