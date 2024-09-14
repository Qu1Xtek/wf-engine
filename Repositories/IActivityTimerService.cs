using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowConfigurator.Models;
using WorkflowConfigurator.Models.Authorization;

namespace WorkflowConfigurator.Repositories;
    public interface IActivityTimerService
    {
        public Task<List<ActivityTimer>> GetAsync();

        public Task CreateAsync(ActivityTimer newTimer);

        public Task RemoveAsync(string id);
    }
