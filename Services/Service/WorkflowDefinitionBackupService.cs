using WorkflowConfigurator.Models.Workflow;
using WorkflowConfigurator.Repositories;

namespace WorkflowConfigurator.Services.Service
{
    public class WorkflowDefinitionBackupService
    {
        private readonly WorkflowDefinitionBackupRepository _workflowDefinitionBackupRepository;
        public WorkflowDefinitionBackupService(WorkflowDefinitionBackupRepository workflowDefinitionRepository)
        {
            _workflowDefinitionBackupRepository = workflowDefinitionRepository;
        }
        public async Task<List<WorkflowDefinitionBackup>> GetAsync() =>
            await _workflowDefinitionBackupRepository.GetAsync();

        public async Task<WorkflowDefinitionBackup> GetAsync(string id) =>
            await _workflowDefinitionBackupRepository.GetAsync(id);

        public async Task CreateAsync(WorkflowDefinitionBackup newWorkflowDefinition) =>
            await _workflowDefinitionBackupRepository.CreateAsync(newWorkflowDefinition);
    }
}
