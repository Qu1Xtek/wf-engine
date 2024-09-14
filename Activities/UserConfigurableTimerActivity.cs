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
    [DefaultDefinition("Activity used to set a Timer based on User input which will not allow Workflow progression unless the configured time has passed")]
    [ActivityTitle("User Set Timer")]
    public class UserConfigurableTimerActivity : Activity, IActivity
    {
        private ActivityTimerRepository _activityTimerService;

        public UserConfigurableTimerActivity(IServiceProvider provider)
        {
            using (var scope = Provider.CreateScope())
            {
                _activityTimerService = scope.ServiceProvider.GetService<ActivityTimerRepository>();
            }
        }

        public string UserMessage { get; set; }

        public ActivityResult Execute(WorkflowContext workflowContext)
        {
            var result = new ActivityResult()
            {
                ResultOutput = UserMessage,
                ActivityState = new ActivityStateResult()
                {
                    ScreenId = "6",
                    Error = new Error { Code = 0, Message = "No error" },
                    Success = true,
                    ActivityMetadata = JsonConvert.SerializeObject(this),
                    ActivityType = nameof(UserConfigurableTimerActivity)
                }
            };

            return Pause(result);
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
                        ActivityType = nameof(UserConfigurableTimerActivity),
                    }
                });
            }

            var timerDuration = workflowContext.ProcessAction.ActivityData.Input;
            workflowContext.WfInstance.SetLastOutput(timerDuration);

            _activityTimerService.Create(new ActivityTimer
            {
                ActiveUntil = DateTime.UtcNow.AddMinutes(long.Parse(timerDuration)),
                WorkflowInstanceId = workflowContext.WfInstance.Id
            });

            var result = new ActivityResult()
            {
                ResultOutput = $"Timer started with settings ({timerDuration}) - {UserMessage}",
                ActivityState = new ActivityStateResult()
                {
                    ScreenId = "2b",
                    Error = new Error { Code = 0, Message = "No error" },
                    Success = true,
                    ActivityMetadata = JsonConvert.SerializeObject(this),
                    ActivityType = nameof(UserConfigurableTimerActivity),
                    Input = timerDuration
                }
            };

            return Hold(result);
        }
    }
}
