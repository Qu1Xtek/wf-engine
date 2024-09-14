using MongoDB.Bson;
using WorkflowConfigurator.Models.Workflow;

namespace WorkflowConfigurator.Repositories
{
    public interface IWorkflowDefinitionRepository
    {
        public Task<List<WorkflowDefinition>> GetAsync();

        public Task<WorkflowDefinition?> GetAsync(string id);
        public Task<WorkflowDefinition?> GetByNameAsync(string name);

        public Task CreateAsync(WorkflowDefinition newBook);

        public Task UpdateAsync(string id, WorkflowDefinition updatedBook);

        public Task RemoveAsync(string id);
    }
}