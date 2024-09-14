using MongoDB.Driver;
using WorkflowConfigurator.Models.Workflow;
using WorkflowConfigurator.Repositories.Workflow;
using WorkflowConfigurator.Services.Helper.Settings;

namespace WorkflowConfigurator.Repositories
{
    public class WorkflowDefinitionBackupRepository
    {
        private readonly TempRepo<WorkflowDefinitionBackup> _tempRepo;

        public WorkflowDefinitionBackupRepository(IConfiguration config)
        {
            _tempRepo = new TempRepo<WorkflowDefinitionBackup>(
                config, 
                Configs.WFDefinitionBackupTable);

        }

        public async Task<List<WorkflowDefinitionBackup>> GetAsync() =>
            await _tempRepo.Collection.Find(_ => true).ToListAsync();

        public async Task<WorkflowDefinitionBackup?> GetAsync(string id) =>
            await _tempRepo.Collection.Find(x => x.WorkflowDefinitionId == id).FirstAsync();

        public async Task CreateAsync(WorkflowDefinitionBackup newWorkflowDefinition) =>
            await _tempRepo.Collection.InsertOneAsync(newWorkflowDefinition);

        //public async Task UpdateAsync(string id, WorkflowDefinitionBackup updatednewWorkflowDefinition) =>
        //    await _workflowDefinitionCollection.ReplaceOneAsync(x => x.Id == id, updatednewWorkflowDefinition);

        //public async Task RemoveAsync(string id) =>
        //    await _workflowDefinitionCollection.DeleteOneAsync(x => x.Id == id);
    }
}
