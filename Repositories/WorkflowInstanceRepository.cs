using MongoDB.Driver;
using WorkflowConfigurator.Models.Workflow;
using WorkflowConfigurator.Services.Helper.Settings;

namespace WorkflowConfigurator.Repositories
{
    public class WorkflowInstanceRepository : BaseMongoRepository<WorkflowInstance>, IWorkflowInstanceService
    {
        public WorkflowInstanceRepository(IConfiguration config) : base(config)
        {
        }

        public async Task<List<WorkflowInstance>> GetAsync() =>
            await _collection.Find(_ => true).ToListAsync();

        public async Task<WorkflowInstance> GetAsync(string id)
        {
            return await _collection.Find(x => x.Id == id).FirstAsync();
        }

        public WorkflowInstance GetById(string id)
        {
            return _collection.Find(x => x.Id == id).First();
        }
        public async Task<WorkflowInstance> GetByNameAsync(string instanceName)
        {
            return await _collection.Find(x => x.InstanceName == instanceName).FirstAsync();
        }

        public async Task<List<WorkflowInstance>> GetByDefinitionId(string definitionId)
        {
            return await _collection.Find(x => x.WorkflowDefinitionId == definitionId).ToListAsync();
        }


        public async Task CreateAsync(WorkflowInstance newWorkflowInstance)
        {
            await _collection.InsertOneAsync(newWorkflowInstance);
        }


        public async Task UpdateAsync(string id, WorkflowInstance updatedWorkflowInstance) =>
            await _collection.ReplaceOneAsync(x => x.Id == id, updatedWorkflowInstance);

        public async Task RemoveAsync(string id) =>
            await _collection.DeleteOneAsync(x => x.Id == id);


        public async Task<string> CleanUserAsync(string email)
        {
            var result = await _collection.DeleteManyAsync(w => w.Owner == email);

            return result.IsAcknowledged.ToString();
        }

        protected override string GetCollectionName(IConfiguration config)
        {
            return config
                .GetSection(Configs.Collections)
                .GetValue<string>(Configs.WFInstanceTable);
        }
    }
}
