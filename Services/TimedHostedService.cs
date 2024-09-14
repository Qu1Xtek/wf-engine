using WorkflowConfiguration.Models;
using WorkflowConfigurator.Models;
using WorkflowConfigurator.Models.Workflow;
using WorkflowConfigurator.Repositories;
using WorkflowConfigurator.Services.Workflow.Execution;
using WorkflowConfigurator.Util;

namespace WorkflowConfigurator.Services
{
    public class TimedHostedService : IHostedService, IDisposable
    {
        private int executionCount = 0;
        private readonly ILogger<TimedHostedService> _logger;
        private IServiceScope _scope;
        private Timer? _timer = null;
        private ActivityTimerRepository _activityTimerService;
        private ArchivedActivityTimerService _archivedActivityTimerService;
        private WorkflowMaster _workflowMaster;
        private readonly IServiceProvider _serviceProvider;

        public TimedHostedService(ILogger<TimedHostedService> logger,
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider!;
            _logger = logger;

        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service running.");

            _scope = _serviceProvider!.CreateScope();
            _activityTimerService = _scope.ServiceProvider.GetRequiredService<ActivityTimerRepository>();
            _archivedActivityTimerService = _scope.ServiceProvider.GetRequiredService<ArchivedActivityTimerService>();
            _workflowMaster = _scope.ServiceProvider.GetRequiredService<WorkflowMaster>();

            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(1));

            return Task.CompletedTask;
        }

        private async void DoWork(object? state)
        {
            try
            {
                var count = Interlocked.Increment(ref executionCount);
                List<ActivityTimer> activityTimers = await _activityTimerService.GetFinishedAsync();
                var activityData = new ActivityData
                {
                    Action = StringConstants.UNHOLD_ACTION
                };

                foreach (ActivityTimer activityTimer in activityTimers)
                {
                    _logger.LogInformation("found for : " + count + " " + DateTime.UtcNow);
                    await _activityTimerService.RemoveAsync(activityTimer.Id);
                    await _archivedActivityTimerService.CreateAsync(activityTimer);
                    await _workflowMaster.UnholdWorkflow(new ExecuteProcessAction() 
                    { 
                        UserEmail = "System", 
                        ActivityData = activityData 
                    }, activityTimer.WorkflowInstanceId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"TimedHostedService error while looping thorugh {nameof(DoWork)}");
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Timed Hosted Service is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _scope?.Dispose();
        }
    }
}
