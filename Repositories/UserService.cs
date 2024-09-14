using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WorkflowConfigurator.Models.Authorization;
using WorkflowConfigurator.Repositories.Workflow;
using WorkflowConfigurator.Services.Helper.Settings;

namespace WorkflowConfigurator.Repositories
{
    public class UserService : IUserService
    {
        private readonly TempRepo<User> _tempRepo;

        public UserService(IConfiguration config)
        {
            _tempRepo = new TempRepo<User>(config, Configs.UsersCollectionTable);
        }

        public async Task<List<User>> GetAsync() =>
            await _tempRepo.Collection.Find(_ => true).ToListAsync();


        public async Task<User> GetOneAsync(string username) =>
            await _tempRepo.Collection.Find(x => x.Email == username).FirstAsync();

        public async Task CreateAsync(User newWorkflowInstance)
        {
            await _tempRepo.Collection.InsertOneAsync(newWorkflowInstance);
        }

        public async Task UpdateAsync(string id, User updatedWorkflowInstance) =>
            await _tempRepo.Collection.ReplaceOneAsync(x => x.Id.ToString() == id, updatedWorkflowInstance);

        public async Task RemoveAsync(string id) =>
            await _tempRepo.Collection.DeleteOneAsync(x => x.Id.ToString() == id);
    }
}
