namespace WorkflowConfigurator.Models.Workflow.Dto
{
    public class WorkflowReportDTO
    {
        public string Id { get; set; }
        
        public string DefinitionId { get; set; }

        public string DefinitionName { get; set; }

        public string WorkflowInstanceName { get; set; }

        public string DefinitionVersion { get; set; }

        public string Status { get; set; }

        public List<StepsResultDTO> Steps { get; set; }

    }
}
