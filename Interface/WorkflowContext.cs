using Newtonsoft.Json;
using WorkflowConfiguration.Models;
using WorkflowConfigurator.Models.Workflow;

namespace WorkflowConfiguration.Interface
{
    public class WorkflowContext
    {
        public object LastExecuted { get; set; }

        public object InputData { get; set; }

        public WorkflowInstance WfInstance { get;set; }

        public ExecuteProcessAction ProcessAction { get; set; }

        public WorkflowContext(object lastExecution, object data, WorkflowInstance wfInstance, ExecuteProcessAction processAction)
        {
            LastExecuted = lastExecution;
            InputData = data;
            WfInstance = wfInstance;
            ProcessAction = processAction;
        }

        public T GetLatestOutput<T>()
        {
#pragma warning disable CS8603 // Possible null reference return.
            return JsonConvert.DeserializeObject<T>(WfInstance.LastOutput);
#pragma warning restore CS8603 // Possible null reference return.
        }
    }
}