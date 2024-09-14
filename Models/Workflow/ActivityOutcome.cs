using WorkflowConfigurator.Models.Activity;

namespace WorkflowConfigurator.Models.Workflow
{
    public class ActivityOutcome
    {
        public int ActivitySourceId { get; set; }
        public PicaviResponse Result { get; set; }

    }
}