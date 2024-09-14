namespace WorkflowConfigurator.Models.Activity
{
    public class ActivityTemplate
    {

        public string Name { get; set; } = String.Empty;

        public string Type { get; set; } = String.Empty;

        public string Description { get; set; } = String.Empty;

        public string[] Outcomes { get; set; }

        public List<ActivityProperty> Properties { get; set; } = new List<ActivityProperty>();
    }
}
