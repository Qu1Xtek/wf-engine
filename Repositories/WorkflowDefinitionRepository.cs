using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowConfiguration.Infrastructure;
using WorkflowConfigurator.Models.Workflow;
using WorkflowConfigurator.Repositories.Workflow;
using WorkflowConfigurator.Services.Helper.Settings;

namespace WorkflowConfigurator.Repositories
{
    public class WorkflowDefinitionRepository : IWorkflowDefinitionRepository
    {
        private readonly TempRepo<WorkflowDefinition> _tempRepo;

        public WorkflowDefinitionRepository(IConfiguration config)
        {
            _tempRepo = new TempRepo<WorkflowDefinition>(
                config,
                Configs.WFDefinitionTable);
        }

        public async Task<List<WorkflowDefinition>> GetAsync() =>
            await _tempRepo.Collection.Find(_ => true).ToListAsync();

        public async Task<WorkflowDefinition> GetAsync(string id) =>
            await _tempRepo.Collection.Find(x => x.Id == id).FirstAsync();

        public WorkflowDefinition Get(string id)
        {
            return _tempRepo.Collection.Find(x => x.Id == id).First();
        }

        public async Task<WorkflowDefinition?> GetByNameAsync(string name) =>
            await _tempRepo.Collection.Find(x => x.Name == name).FirstOrDefaultAsync();

        public async Task CreateAsync(WorkflowDefinition newWorkflowDefinition) =>
            await _tempRepo.Collection.InsertOneAsync(newWorkflowDefinition);

        public async Task UpdateAsync(string id, WorkflowDefinition updatednewWorkflowDefinition) =>
            await _tempRepo.Collection.ReplaceOneAsync(x => x.Id == id, updatednewWorkflowDefinition);

        public async Task RemoveAsync(string id) =>
            await _tempRepo.Collection.DeleteOneAsync(x => x.Id == id);
    }
}