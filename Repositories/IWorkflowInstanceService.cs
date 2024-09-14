using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowConfigurator.Models.Workflow;

namespace WorkflowConfigurator.Repositories
{
    public interface IWorkflowInstanceService
    {
        public Task<List<WorkflowInstance>> GetAsync();

        public Task<WorkflowInstance?> GetAsync(string id);

        public Task CreateAsync(WorkflowInstance newBook);

        public Task UpdateAsync(string id, WorkflowInstance updatedBook);

        public Task RemoveAsync(string id);

        public Task<string> CleanUserAsync(string id);
    }
}
