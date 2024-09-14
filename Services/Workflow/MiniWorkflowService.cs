using WorkflowConfigurator.Models.Activity;
using WorkflowConfigurator.Models.Workflow;
using WorkflowConfigurator.Repositories;

namespace WorkflowConfigurator.Services.Workflow
{
    public class MiniWorkflowService
    {
        private readonly WorkflowDefinitionRepository _workflowDefinitionService;

        public MiniWorkflowService(WorkflowDefinitionRepository workflowDefinitionService) {

            _workflowDefinitionService = workflowDefinitionService;
        }

        public async Task<WorkflowDefinitionBackup> GetMiniDefinition()
        {
            return null;
        }

        public async Task<ActivityDefinition> GetNextAcitivty(string wfDefinitionId, int currentStep, string? outcome = null)
        {
            var wfDef = await _workflowDefinitionService.GetAsync(wfDefinitionId);
            var wfVerData = wfDef.WorkflowDefinitionVersionData[wfDef.Version];

            if (currentStep == -2)
            {
                return null;
            }

            if (currentStep == -1)
            {
                return wfVerData.Activities.First(a => a.ActivityId == wfVerData.StartingActivity);
            }

            if (outcome == Outcomes.AWAIT_RESUME.ToString() || outcome == Outcomes.AWAIT_EXECUTE.ToString() || outcome == Outcomes.ONHOLD.ToString())
            {
                return wfVerData
                .Activities
                .First(a => a.ActivityId == currentStep);
            }

            ActivityConnection? connection = wfVerData
                .ActivityConnections
                .FirstOrDefault(ac => ac.ActivitySourceId == currentStep);

            if (connection == null) 
            {
                return null;
            }

            return wfVerData
                .Activities
                .First(a => a.ActivityId == connection.ActivityIdTarget[outcome]);
        }

        public async Task<bool> IsMiniWFNotExecuting(int currentActivityId)
        {
            var miniwf = await _workflowDefinitionService.GetAsync("64df6d71436294e00dffd8ba");
            ActivityConnection? connection = (miniwf)
                .WorkflowDefinitionVersionData[miniwf.Version]
                .ActivityConnections
                .FirstOrDefault(ac => ac.ActivitySourceId == currentActivityId);

            return connection == null;
        }
    }
}
