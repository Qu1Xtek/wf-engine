using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowConfiguration.Infrastructure;
using WorkflowConfigurator.Models.Authorization;
using WorkflowConfigurator.Repositories.Workflow;
using WorkflowConfigurator.Services.Helper.Settings;

namespace WorkflowConfigurator.Repositories
{
    public class UserSessionService : IUserSessionService
    {
        private readonly TempRepo<UserSession> _tempRepo;

        public UserSessionService(IConfiguration config)
        {
            _tempRepo = new TempRepo<UserSession>(config, Configs.UserSessionsCollectionTable);
        }

        public async Task<List<UserSession>> GetAsync() =>
            await _tempRepo.Collection.Find(_ => true).ToListAsync();

        public async Task<UserSession> GetAsync(string userEmail)
        {
            UserSession session = await _tempRepo.Collection.Find(x => x.Username == userEmail).FirstOrDefaultAsync();

            if (session == null)
            {
                session = new UserSession() { Username = userEmail, WorkflowDefinitionId = "", WorkflowInstanceId = "" };
                await CreateAsync(session);
            }

            return session;
        }

        public async Task<UserSession> GetAsyncWithNoCreate(string userEmail)
        {
            UserSession session = await _tempRepo.Collection.Find(x => x.Username == userEmail).FirstOrDefaultAsync();

            return session;
        }

        public UserSession GetByUser(string userName)
        {
            UserSession session = _tempRepo.Collection.Find(x => x.Username == userName).FirstOrDefault();

            if (session == null)
            {
                session = new UserSession() { Username = userName, WorkflowDefinitionId = "", WorkflowInstanceId = "" };
                Create(session);
            }

            return session;
        }


        public async Task CreateAsync(UserSession newWorkflowInstance)
        {

            await _tempRepo.Collection.InsertOneAsync(newWorkflowInstance);
        }

        public void Create(UserSession newWorkflowInstance)
        {
            _tempRepo.Collection.InsertOne(newWorkflowInstance);
        }

        public async Task ResetAsync(string id, UserSession session)
        {
            session.WorkflowDefinitionId = "";
            session.WorkflowInstanceId = "";

            await _tempRepo.Collection.ReplaceOneAsync(x => x.Id == id, session);
        }

        public async Task<string> RemovePerUserAsync(string email)
        {
            await _tempRepo.Collection.DeleteOneAsync(x => x.Username == email);
            return "OK";
        }

        public async Task UpdateAsync(string id, UserSession updatedWorkflowInstance) =>
            await _tempRepo.Collection.ReplaceOneAsync(x => x.Id == id, updatedWorkflowInstance);
        public void Update(string id, UserSession updatedWorkflowInstance) =>
            _tempRepo.Collection.ReplaceOne(x => x.Id == id, updatedWorkflowInstance);

        public async Task RemoveAsync(string id) =>
            await _tempRepo.Collection.DeleteOneAsync(x => x.Id.ToString() == id);

        public async Task RemoveByWorkflowDefinitionId(string workflowDefinitionId)
        {
            await _tempRepo.Collection.DeleteManyAsync(x => x.WorkflowDefinitionId == workflowDefinitionId);
        }
    }
}
