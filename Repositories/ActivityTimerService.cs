using MongoDB.Driver;
using WorkflowConfiguration.Infrastructure;
using WorkflowConfigurator.Models;
using WorkflowConfigurator.Repositories.Workflow;
using WorkflowConfigurator.Services.Helper.Settings;

namespace WorkflowConfigurator.Repositories
{
    public class ActivityTimerRepository : IActivityTimerService
    {
        private readonly TempRepo<ActivityTimer> _tempRepo;

        public ActivityTimerRepository(IConfiguration config)
        {
            // This should be transfered to it's own repo, temp is used only TEMPORARY
            _tempRepo = new TempRepo<ActivityTimer>(config, Configs.ActivityTimerCollectionTable);
        }

        public async Task<List<ActivityTimer>> GetAsync() =>
            await _tempRepo.Collection.Find(_ => true).ToListAsync();

        public async Task<List<ActivityTimer>> GetFinishedAsync() =>
            await _tempRepo.Collection.Find(timer => timer.ActiveUntil < DateTime.UtcNow).ToListAsync();

        public async Task<ActivityTimer> GetByWorkflowInstanceIdAsync(string workflowInstanceId) =>
            await _tempRepo.Collection.Find(timer => timer.WorkflowInstanceId == workflowInstanceId).FirstAsync();
 
        public async Task CreateAsync(ActivityTimer newWorkflowInstance)
        {
            await _tempRepo.Collection.InsertOneAsync(newWorkflowInstance);
        }

        public void Create(ActivityTimer newWorkflowInstance)
        {

            _tempRepo.Collection.InsertOne(newWorkflowInstance);
        }

        public async Task RemoveAsync(string id) =>
            await _tempRepo.Collection.DeleteOneAsync(x => x.Id == id);
    }
}
