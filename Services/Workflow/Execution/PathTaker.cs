using WorkflowConfiguration.Interface;
using WorkflowConfigurator.Activities;
using WorkflowConfigurator.Models;
using WorkflowConfigurator.Models.Activity;
using WorkflowConfigurator.Models.Workflow;
using WorkflowConfigurator.Repositories;

namespace WorkflowConfigurator.Services.Workflow.Execution
{
    public class PathTaker
    {
        private readonly ActivityService _activityService;
        private readonly WorkflowDefinitionRepository _wfDefService;

        public PathTaker(WorkflowDefinitionRepository wfDefService, ActivityService activityService)
        {
            _activityService = activityService;
            _wfDefService = wfDefService;
        }

        public async Task<GRes<WorkflowDefinitionData>> CheckForMiniWF(
            WorkflowDefinitionData parentWfDefinitionData,
            WorkflowInstance wfInstance,
            int currentStep)
        {
            var activityToExecute = GetActivityToExecute(parentWfDefinitionData, currentStep);

            if (activityToExecute.ActivityType == nameof(MiniWFActivity))
            {
                var miniWF = _activityService.CreateInstanceByActivityType<MiniWFActivity>(
                    activityToExecute);

                var miniWfDef = await _wfDefService.GetByNameAsync(miniWF.WfDefinitionId);
                var versionData = miniWfDef.WorkflowDefinitionVersionData[miniWfDef.Version];

                return GRes<WorkflowDefinitionData>.OK(versionData, string.Empty);
            }

            return GRes<WorkflowDefinitionData>.Fault<WorkflowDefinitionData>(
                null, string.Empty, -1);
        }

        public ActivityDefinition GetActivityToExecute(WorkflowDefinitionData wfDefinitionData, int currentStep)
        {
            ActivityDefinition activity = wfDefinitionData.Activities.Single(a
                => a.ActivityId == currentStep);

            return activity;
        }

        public WfPath ProceedToNextStep(WorkflowInstance workflowInstance,
            WorkflowDefinitionData workflowDefinitionData,
            ActivityResult activityResult)
        {

            if (activityResult.Outcome == Outcomes.AWAIT_RESUME.ToString()
                || activityResult.Outcome == Outcomes.AWAIT_EXECUTE.ToString()
                || activityResult.Outcome == Outcomes.ONHOLD.ToString()
                || activityResult.Outcome == Outcomes.WF_COMPLETED.ToString()
                || activityResult.Outcome == Outcomes.FAULT.ToString())
            {
                // No need to move next, WF will be paused
                return new WfPath(workflowInstance.CurrentActivityId,
                    activityResult.Outcome,
                    activityResult.ActivityState,
                    workflowInstance.MiniWFStep);
            }
            else
            {
                var stepId = workflowInstance.MiniWFStep == -1 
                    ? workflowInstance.CurrentActivityId
                    : workflowInstance.MiniWFStep;

                var connection = workflowDefinitionData.GetConnectionFromSource(stepId);

                // Check if next connection exists
                if (connection == null)
                {
                    // If inside a mini workflow, exit from mini WF
                    if (workflowInstance.MiniWFStep != -1)
                    {
                        var path = new WfPath(workflowInstance.CurrentActivityId,
                            activityResult.Outcome,
                            activityResult.ActivityState);

                        path.MiniWfId = -2;

                        return path;
                    }
                    
                    // Prepare and return dummy workflow state for workflow completion
                    return TakeFinalStep(workflowInstance, activityResult.ActivityState);
                }

                var nextStep = connection.GetTargetId(activityResult.Outcome);
                // Normal WF processing, going to the next step if available
                return new WfPath(
                    workflowInstance.MiniWFStep == -1 ? nextStep : workflowInstance.CurrentActivityId,
                    activityResult.Outcome,
                    activityResult.ActivityState,
                    nextStep);
            }
        }

        private WfPath TakeFinalStep(WorkflowInstance workflowInstance, ActivityStateResult result)
        {
            return new WfPath(workflowInstance.CurrentActivityId,
                Outcomes.FINAL_ACTIVITY.ToString(),
                result);
        }
    }

    public class WfPath
    {
        public WfPath(int nextId, string outcome, ActivityStateResult stateResult, int miniWfStep = -1)
        {
            Outcome = outcome;
            NextActivityId = nextId;
            ActivityStateResult = stateResult;
            MiniWfId = miniWfStep;
        }

        public int NextActivityId { get; set; }

        public int MiniWfId { get; set; }

        public ActivityStateResult ActivityStateResult { get; set; }

        public string Outcome { get; set; }
    }
}
