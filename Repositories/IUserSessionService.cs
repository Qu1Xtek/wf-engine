using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowConfigurator.Models.Authorization;

namespace WorkflowConfigurator.Repositories
{
    public interface IUserSessionService
    {
        public Task<List<UserSession>> GetAsync();

        public Task<UserSession?> GetAsync(string userId);

        public Task CreateAsync(UserSession newBook);

        public Task UpdateAsync(string id, UserSession updatedBook);

        public Task RemoveAsync(string id);
    }
}
