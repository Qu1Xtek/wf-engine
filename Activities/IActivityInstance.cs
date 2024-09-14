using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkflowConfigurator.Models.Activity;

namespace WorkflowConfiguration.Activities
{
    public interface IActivityInstance
    {
        ActivityResult Execute(object input);
    }
}
