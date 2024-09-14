namespace WorkflowConfigurator.Models.Workflow.Dto
{
    public class WorkflowDefinitionDto
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public int? Version { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? Creator { get; set; }
        public string? Reviewer { get; set; }
        public List<ActivityDefinition> Activities { get; set; }
        public List<ActivityConnection> ActivityConnections { get; set; }
        public ActivityDefinition StartingDefinition { get; set; }
        public int StartingActivity { get; set; }
        public string? CompanyId { get; set; }
        public bool IsMiniWf { get; set; }

        public WorkflowDefinitionDto? Draft {get; set;}

    }
}
