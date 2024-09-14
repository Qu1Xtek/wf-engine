using MongoDB.Driver;
using WorkflowConfigurator.Models;
using WorkflowConfigurator.Repositories.Workflow;
using WorkflowConfigurator.Services.Helper.Settings;

namespace WorkflowConfigurator.Repositories
{
    public class ArchivedActivityTimerService : IActivityTimerService
    {
        private readonly TempRepo<ActivityTimer> _tempRepo;

        public ArchivedActivityTimerService(IConfiguration config)
        {
            _tempRepo = new TempRepo<ActivityTimer>(config, Configs.ArchivedActivityTimerCollectionTable);
        }

        public async Task<List<ActivityTimer>> GetAsync() =>
            await _tempRepo.Collection.Find(_ => true).ToListAsync();

        public async Task CreateAsync(ActivityTimer newWorkflowInstance)
        {
            await _tempRepo.Collection.InsertOneAsync(newWorkflowInstance);
        }

        public async Task RemoveAsync(string id) =>
            await _tempRepo.Collection.DeleteOneAsync(x => x.Id.ToString() == id);
    }
}
