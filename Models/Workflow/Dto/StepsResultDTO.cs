using WorkflowConfigurator.Models.Activity;

namespace WorkflowConfigurator.Models.Workflow.Dto
{
    public class StepsResultDTO
    {
        public StepsResultDTO() { }

        public StepsResultDTO(ActivityDefinition activityDef,
            ActivityResult result) 
        {
            Title = activityDef.Title;
            Description = activityDef.Description;
            Input = result.ActivityState.Input;
            ActivityMetadata = result.ActivityState.ActivityMetadata;
            ActivityType = result.ActivityState.ActivityType;
            Outcome = result.Outcome;
            Result = result.ResultOutput;
            ActivityId = result.ActivitySource;
            ExecutedBy = result.ExecutedBy;
            ExecutionTime = $"{result.ExecutionTime.ToShortDateString()} - {result.ExecutionTime.ToShortTimeString()}";
        }

        public int ActivityId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string ActivityType { get; set; }

        public string? ActivityMetadata { get; set; }

        public string Input { get; set; }

        public string Result { get; set; }

        public string Outcome { get; set; }

        public string ExecutionTime { get; set; }

        public string ExecutedBy { get; set; }
    }
}