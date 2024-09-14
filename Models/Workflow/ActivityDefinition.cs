namespace WorkflowConfigurator.Models.Workflow
{
    public class ActivityDefinition
    {
        public ActivityDefinition() { }

        public int ActivityId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string ActivityType { get; set; }

        public string? ActivityMetadata { get; set; }

        public float X { get; set; }

        public float Y { get; set; }
    }
}