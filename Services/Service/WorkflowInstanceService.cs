using WorkflowConfigurator.Models.Workflow;
using WorkflowConfigurator.Repositories;
using WorkflowConfigurator.Services.DIP;

namespace WorkflowConfigurator.Services.Service
{
    public class WorkflowInstanceService
    {

        private readonly WorkflowInstanceRepository _workflowInstanceRepository;
        private readonly ActivityTimerRepository _activityTimerRepository;
        private readonly WorkflowDefinitionService _workflowDefinitionService;
        private readonly DIPService _dipService;

        public WorkflowInstanceService(WorkflowInstanceRepository workflowInstanceRepository,
            ActivityTimerRepository activityTimerRepository,
            WorkflowDefinitionService workflowDefinitionService,
            DIPService dipService)
        {
            _workflowInstanceRepository = workflowInstanceRepository;
            _activityTimerRepository = activityTimerRepository;
            _workflowDefinitionService = workflowDefinitionService;
            _dipService = dipService;
        }



        public async Task<List<WorkflowInstance>> GetUnfinishedOrderedByStatusAsync(string workflowDefinitionId)
        {
            List<WorkflowInstance> instances = await _workflowInstanceRepository.GetByDefinitionId(workflowDefinitionId);

            return instances.FindAll(x => x.Status.ToString() != WorkflowStatus.FINISHED.ToString())
                .OrderBy(x => x.Status.ToString() != WorkflowStatus.ONHOLD.ToString())
                .ThenBy(x => x.Status.ToString())
                .ThenBy(x => x.CreatedOn)
                .ToList();
        }


        public async Task Create(WorkflowInstance newWorkflowInstance)
        {
            await _workflowInstanceRepository.CreateAsync(newWorkflowInstance);
            await _dipService.CreateWorkflowInstanceRecord(newWorkflowInstance);
        }

        public async Task<WorkflowInstance> CreateFromDefinition(string definitionId, string email)
        {
            WorkflowDefinition? workflowDefinition = await _workflowDefinitionService.GetAsync(definitionId);
            WorkflowDefinitionData data = workflowDefinition.WorkflowDefinitionVersionData[workflowDefinition.Version];

            var workflowInstance = new WorkflowInstance()
            {
                WorkflowDefinitionId = definitionId,
                CurrentActivityId = data.StartingActivity,
                Owner = email,
                CompanyId = workflowDefinition.CompanyId
            };

            await Create(workflowInstance);

            return workflowInstance;
        }

        public async Task UpdateAsync(string id, WorkflowInstance updatedWorkflowInstance)
        {
            await _workflowInstanceRepository.UpdateAsync(id, updatedWorkflowInstance);
            await _dipService.UpdateWorkflowInstanceRecord(updatedWorkflowInstance);
        }


        public async Task RemoveAsync(string id) =>
             await _workflowInstanceRepository.RemoveAsync(id);


        public async Task<WorkflowInstance> GetByNameAsync(string instanceName)
        {
            return await _workflowInstanceRepository.GetByNameAsync(instanceName);
        }

        public async Task<WorkflowInstance> GetByIdAsync(string id)
        {
            return await _workflowInstanceRepository.GetAsync(id);
        }

        public string GenerateInstanceName(string user, string workflowDefinitionName)
        {
            var datetimeNowHoursAndMinutes = DateTime.UtcNow.ToString("HH:mm").Replace(":", "");

            // Add random number at the end of the new worflowId with 4 digits
            return (user + "_" + workflowDefinitionName + "_" + datetimeNowHoursAndMinutes + "_" + new Random().Next()).Replace(" ", "");
        }

        public async Task<string> CleanUserAsync(string email)
        {
            return await _workflowInstanceRepository.CleanUserAsync(email);
        }
    }
}
