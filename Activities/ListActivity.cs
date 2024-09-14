using WorkflowConfiguration.Models;

namespace WorkflowConfigurator.Activities
{
    public class ListActivity
    {
        public string UserMessage { get; set; }
        public List<ActionData> ListValues { get; set; } = new List<ActionData>();

        public ListActivity(string userMessage, List<ActionData> values)
        {
            this.UserMessage = userMessage;
            this.ListValues = values;
        }

    }
}
