using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowConfigurator.Models.Authorization;

namespace WorkflowConfigurator.Repositories
{
    public interface IUserService
    {
        public Task<List<User>> GetAsync();

        public Task CreateAsync(User newBook);

        public Task UpdateAsync(string id, User updatedBook);

        public Task RemoveAsync(string id);
    }
}
