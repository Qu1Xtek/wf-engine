using WorkflowConfiguration.Activities;

namespace WorkflowConfigurator.Models.Activity
{
    public class ActivityResult
    {
        /// <summary>
        ///  Common constructor for general result with basic data
        /// </summary>
        /// <param name="execBy"></param>
        /// <param name=""></param>
        public ActivityResult(string execBy, string output)
        {
            ExecutedBy = execBy;
            Outcome = output;
        }

        public ActivityResult()
        {
            
        }

        public string Outcome { get; set; }

        public ActivityStateResult ActivityState { get; set; }

        public int ActivitySource { get; set; }

        public string ResultOutput { get; set; }

        public DateTime ExecutionTime { get; set; }

        public string ExecutedBy { get; set; }

        public void ApplyData(string executedBy, DateTime executionTime, int activityId, string outcome)
        {
            Outcome = outcome;
            ActivitySource = activityId;
            ExecutedBy = executedBy;
            ExecutionTime = executionTime;
        }
    }
}