using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowConfiguration.Infrastructure;
using WorkflowConfigurator.Models.Activity;

namespace WorkflowConfiguration.Interface
{
    public interface IActivity
    {
        ActivityResult Execute(WorkflowContext workflowContext);
        ActivityResult Resume(WorkflowContext workflowContext);
    }
}
