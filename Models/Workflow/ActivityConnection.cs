namespace WorkflowConfigurator.Models.Workflow
{
    public class ActivityConnection
    {
        public Dictionary<string,int> ActivityIdTarget { get; set; }

        public int ActivitySourceId { get; set; }

        public int GetTargetId(string outcome)
        {
            return ActivityIdTarget[outcome];
        }
    }
}
