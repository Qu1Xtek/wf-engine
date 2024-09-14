using WorkflowConfiguration.Activities;
using WorkflowConfiguration.Interface;
using WorkflowConfigurator.Attributes;

using WorkflowConfigurator.Services.Workflow.Execution;

namespace WorkflowConfigurator.Models.Activity
{
    [PossibleOutcomes($"DONE")]
    public class Activity
    {
        public static IServiceProvider Provider { get; set; }

        private readonly HotkeyHandler _hotkeyHandler;

        public Activity()
        {
            // Create middlware to keep needed data in a scoped service from which
            // we will resolve the userId here and assign it to the result

            using (var scope = Provider.CreateScope())
            {
                _hotkeyHandler = scope.ServiceProvider.GetService<HotkeyHandler>();
            }
        }

        protected ActivityResult Done(ActivityResult result)
        {
            if ( result == null )
            {
                result = new ActivityResult();
            }

            result.Outcome = Outcomes.DONE.ToString();

            return result;
        }

        protected ActivityResult Pause(ActivityResult result)
        {
            if (result == null)
            {
                result = new ActivityResult();
            }

            result.Outcome = Outcomes.AWAIT_RESUME.ToString();

            return result;
        }

        protected ActivityResult Hold(ActivityResult result)
        {
            if (result == null)
            {
                result = new ActivityResult();
            }

            result.Outcome = Outcomes.ONHOLD.ToString();

            return result;
        }

        protected ActivityResult Outcome(string specificOutcome, ActivityResult result)
        {
            if (result == null)
            {
                result = new ActivityResult();
            }

            result.Outcome = specificOutcome;

            return result;
        }

        public virtual ActivityResult Resume(WorkflowContext workflowContext)
        {
            return new ActivityResult();
        }

        protected ActivityResult HandleHotkey(WorkflowContext context, bool toLastScreen = false)
        {
            return _hotkeyHandler.HandleHotkey(context, toLastScreen);
        }
    }
}
