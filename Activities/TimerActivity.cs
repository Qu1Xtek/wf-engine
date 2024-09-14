using Newtonsoft.Json;
using WorkflowConfiguration.Interface;
using WorkflowConfigurator.Attributes;
using WorkflowConfigurator.Models;
using WorkflowConfigurator.Models.Activity;
using WorkflowConfigurator.Repositories;
using WorkflowConfigurator.Util;

namespace WorkflowConfiguration.Activities
{
    [PossibleOutcomes("DONE")]
    [DefaultDefinition("Activity used to set a Timer which will not allow Workflow progression unless the configured time has passed")]
    [ActivityTitle("Timer")]
    public class TimerActivity : Activity, IActivity
    {
        private ActivityTimerRepository _activityTimerService;

        public TimerActivity(IServiceProvider provider)
        {
            using (var scope = Provider.CreateScope())
            {
                _activityTimerService = scope.ServiceProvider.GetService<ActivityTimerRepository>();
            }
        }

        public string UserMessage { get; set; }

        public int TimerDuration { get; set; }

        [JsonProperty("TimerEnds")]
        private DateTime _timerEndTime { get; set; }

        public ActivityResult Execute(WorkflowContext workflowContext)
        {
            DateTime timerActiveUntil = DateTime.UtcNow.AddMinutes(TimerDuration);

            _activityTimerService.Create(new ActivityTimer 
            { 
                ActiveUntil = timerActiveUntil, 
                WorkflowInstanceId = workflowContext.WfInstance.Id 
            });

            _timerEndTime = timerActiveUntil;

            return Hold(GetState());
        }


        public override ActivityResult Resume(WorkflowContext workflowContext)
        {
            // Unhold workflow and continue execution
            if (workflowContext.ProcessAction?.ActivityData?.Action?.ToUpper() == StringConstants.UNHOLD_ACTION)
            {
                return Done(new ActivityResult()
                {
                    ActivityState = new ActivityStateResult()
                    {
                        Error = new Error { Code = 0, Message = "No error" },
                        Success = true,
                        ActivityType = nameof(TimerActivity),
                    }
                });

            }

            return base.Resume(workflowContext);
        }

        private ActivityResult GetState()
        {
            return new ActivityResult()
            {
                ResultOutput = $" Timer started with settings ({TimerDuration}) - {UserMessage}",
                ActivityState = new ActivityStateResult()
                {
                    ScreenId = "8",
                    Error = new Error { Code = 0, Message = "No error" },
                    Success = true,
                    ActivityMetadata = JsonConvert.SerializeObject(this),
                    ActivityType = nameof(TimerActivity)
                }
            };
        }
    }
}
