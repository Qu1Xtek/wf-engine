namespace WorkflowConfigurator.Models.Workflow
{
    public class WorkflowDefinitionData
    {
        public List<ActivityDefinition> Activities { get; set; }
        public List<ActivityConnection> ActivityConnections { get; set; }
        public ActivityDefinition StartingDefinition { get; set; }
        public int StartingActivity { get; set; }

        public ActivityConnection? GetConnectionFromSource(int sourceId)
        {
            return ActivityConnections
                .FirstOrDefault(ac => ac.ActivitySourceId == sourceId);
        }
    }
}
