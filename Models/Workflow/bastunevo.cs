using Amazon.SimpleWorkflow.Model;
using WorkflowConfiguration.Interface;
using WorkflowConfiguration.Models;
using WorkflowConfigurator.Activities;
using WorkflowConfigurator.Models.Activity;
using WorkflowConfigurator.Models.Authorization;
using WorkflowConfigurator.Repositories;
using WorkflowConfigurator.Services.DIP;
using WorkflowConfigurator.Services.PicaviTranslator;
using WorkflowConfigurator.Services.Workflow;

namespace WorkflowConfigurator.Models.Workflow
{
    public class bastunevo
    {
        private readonly IActivityService _activityService;

        private readonly WorkflowDefinitionRepository _workflowDefinitionService;
        private readonly WorkflowInstanceRepository _workflowInstanceService;
        private readonly ILogger<bastunevo> _logger;
        private readonly DIPService _dipService;
        private readonly ScreenTranslateService _screenTranslateService;
        private readonly UserSessionService _userSessionService;
        private readonly InstanceMaintainer _instanceConfigurator;
        private readonly MiniWorkflowService _miniWfService;

        public bastunevo(WorkflowDefinitionRepository workflowDefinitionService,
            UserSessionService userSessionService,
            WorkflowInstanceRepository workflowInstanceService,
            ILogger<bastunevo> logger,
            DIPService dipService,
            ScreenTranslateService screenTranslateService,
            InstanceMaintainer instanceConfigurator,
            MiniWorkflowService miniWfService,
            ActivityService activityService)
        {
            _workflowDefinitionService = workflowDefinitionService;
            _workflowInstanceService = workflowInstanceService;
            _logger = logger;
            _dipService = dipService;
            _screenTranslateService = screenTranslateService;
            _userSessionService = userSessionService;
            _instanceConfigurator = instanceConfigurator;
            _miniWfService = miniWfService;
            _activityService = activityService;
        }

        public async Task<ActivityStateResult> ProgressWorkflow(ExecuteProcessAction executeProcessAction, string workflowInstanceName)
        {
            WorkflowInstance workflowInstance = await _workflowInstanceService.GetAsync(workflowInstanceName);

            if (workflowInstance.Status != WorkflowStatus.ONHOLD)
            {
                return await ProcessWorkflow(workflowInstance, executeProcessAction, workflowInstanceName);
            }
            else
            {
                if (executeProcessAction.ActivityData.Hotkey == "Back")
                {
                    UserSession session = await _userSessionService.GetAsync(executeProcessAction.UserEmail);

                    await _userSessionService.ResetAsync(session.Id, session);

                    return null;
                }

                throw new InvalidOperationException(
                    $"Invalid action for the workflow instance, " +
                    $"progress workflow should not be called " +
                    $"when status is {workflowInstance.Status}");
            }
        }

        private async Task<ActivityStateResult> ProcessWorkflow(WorkflowInstance wfInstance, ExecuteProcessAction executeProcessAction, string workflowInstanceId)
        {
            WorkflowDefinition workflowDefinition = await _workflowDefinitionService.GetAsync(wfInstance.WorkflowDefinitionId);
            return await ExecuteNextStep(executeProcessAction, wfInstance, workflowDefinition);
        }

        public async Task<ActivityStateResult> UnholdWorkflow(
            ExecuteProcessAction executeProcessAction,
            string workflowInstanceId)
        {
            try
            {

                //tuka se produljava holdnat workflow kato tova se vikat ot TimedService-a ako vidi taimera che mu e vreme
                WorkflowInstance workflowInstance = await _workflowInstanceService.GetAsync(workflowInstanceId);

                if (workflowInstance.Status == WorkflowStatus.ONHOLD)
                {
                    return await ProcessWorkflow(workflowInstance, executeProcessAction, workflowInstanceId);
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Invalid action for the workflow instance, " +
                        $"uhold workfow should be called only " +
                        $"when status is OnHold");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error {ex.Message}, " +
                    $"{nameof(UnholdWorkflow)} with params wfId: {workflowInstanceId} and " +
                    $"action {executeProcessAction?.ActivityData?.ActionData}");

                throw ex;
            }

        }

        private async Task<ActivityStateResult> ExecuteNextStep(
            ExecuteProcessAction executeProcessAction,
            WorkflowInstance workflowInstance,
            WorkflowDefinition workflowDefinition)
        {
            WorkflowDefinitionData workflowDefinitionData = workflowDefinition
                .WorkflowDefinitionVersionData[workflowDefinition.Version];

            //try
            //{
            //    await _dipService.CheckDefinitionHash(workflowDefinition);
            //}
            //catch (Exception ex)
            //{
            //    return new ActivityStateResult()
            //    {
            //        Success = false,
            //        Error = new Error()
            //        {
            //            Code = 1,
            //            Message = ex.Message
            //        }
            //    };
            //}

            try
            {

                // tuka veche si vzel version datata i pochvash da proverqvash, ako e final activity, da go prikluchish workflowa, ako ne e vzima sledvashtoto aktiviti za izpulnenie
                if (workflowInstance.Status == WorkflowStatus.FINAL_ACTIVITY)
                {
                    return await CompleteWorkflow(workflowInstance, executeProcessAction.UserEmail);
                }

                ActivityDefinition activity = workflowDefinitionData
                    .Activities
                    .Single(a => a.ActivityId == workflowInstance.CurrentActivityId);

                if (activity.ActivityType == nameof(MiniWFActivity))
                {
                    MiniWFActivity miniWFActivity = (MiniWFActivity)_activityService.CreateInstanceByActivityType<IActivity>(
                        activity
                        );

                    ActivityDefinition nextActivity = await _miniWfService.GetNextAcitivty(
                        miniWFActivity.WfDefinitionId,
                        workflowInstance.MiniWFStep,
                        workflowInstance.ActivityResults?.Last().Outcome);


                    if (nextActivity != null)
                    {
                        activity = nextActivity;
                        workflowInstance.MiniWFStep = nextActivity.ActivityId;
                    }
                    else
                    {
                        ActivityConnection? connection = workflowDefinitionData
                            .ActivityConnections
                            .FirstOrDefault(ac => ac.ActivitySourceId == workflowInstance.CurrentActivityId);

                        workflowInstance.MiniWFStep = -2;

                        if (connection == null)
                        {
                            return await PrepareFinalActivity(workflowInstance);
                        }

                        activity = workflowDefinitionData.Activities.Find(a => a.ActivityId == connection.ActivityIdTarget[Outcomes.DONE.ToString()]);
                    }
                }

                ActivityResult activityResult = null;


                // pochva da proverqva - purviq if e ako veche e minal execute na activity ili resume (v sluchaq na scan activity) i pochva da mu pravi resume
                if (workflowInstance.Status == WorkflowStatus.AWAIT_EXECUTE
                    || workflowInstance.Status == WorkflowStatus.AWAIT_RESUME)
                {
                    activityResult = ResumeActivity(workflowInstance, activity, executeProcessAction);

                    activityResult.ExecutedBy = executeProcessAction.UserEmail;
                    activityResult.ExecutionTime = DateTime.UtcNow;
                    activityResult.ActivitySource = workflowInstance.CurrentActivityId;

                    workflowInstance.ActivityResults.Add(activityResult);

                    if (activityResult.Outcome == Outcomes.AWAIT_RESUME.ToString())
                    {
                        //ako ima oshte za izpulnenie ne smenq stepa, samo zapisva rezultata i go ostavq v toq status
                        await UpdateState(workflowInstance, WorkflowStatus.AWAIT_RESUME, activityResult.ActivityState);
                        return (activityResult.ActivityState);
                    }
                    else if (activityResult.Outcome == Outcomes.ONHOLD.ToString())
                    {
                        // ako e on hold sushtata shema
                        await UpdateState(workflowInstance, WorkflowStatus.ONHOLD, activityResult.ActivityState);
                        return activityResult.ActivityState;
                    }

                    if (workflowInstance.MiniWFStep == -2) // Just finished
                    {
                        ActivityConnection? connection = workflowDefinitionData
                        .ActivityConnections
                        .FirstOrDefault(ac => ac.ActivitySourceId == activity.ActivityId);

                        if (connection == null)
                        {
                            return await PrepareFinalActivity(workflowInstance);
                        }

                        workflowInstance.MiniWFStep = -1; // Clear just finished state
                        workflowInstance.CurrentActivityId = activity.ActivityId;

                        await UpdateState(workflowInstance,
                       WorkflowStatus.RUNNING,
                       activityResult.ActivityState,
                       activity.ActivityId);
                    }
                    else
                    {
                        if (workflowInstance.MiniWFStep == -1) // Normal execution without MINIWF
                        {
                            ActivityConnection? connection = workflowDefinitionData
                            .ActivityConnections
                            .FirstOrDefault(ac => ac.ActivitySourceId == activity.ActivityId);


                            if (connection == null)
                            {
                                return await PrepareFinalActivity(workflowInstance);
                            }

                            await UpdateState(workflowInstance,
                           WorkflowStatus.RUNNING,
                           activityResult.ActivityState,
                           connection.ActivityIdTarget[activityResult.Outcome]);
                        }
                        else
                        {
                            await UpdateState(workflowInstance,
                            WorkflowStatus.RUNNING,
                            activityResult.ActivityState,
                            null);
                        }
                    }



                    // tuka e samo ako e v drug status t.e. running i vzima sledvashtoto aktiviti za izpulnenie
                    activity = workflowDefinitionData
                        .Activities
                        .Single(a => a.ActivityId == workflowInstance.CurrentActivityId);
                }
                else if (workflowInstance.Status == WorkflowStatus.ONHOLD)
                {
                    // tva e vtoriq if, t.e. ako ne e await execute ili resume ami e onhold , hendli taq tupotiq, kato predpolagame che mu e doshlo vreme da se unholdne shtom mu e viknat toq metod (UnholdWorkflow)
                    ActivityConnection? connection = workflowDefinitionData.ActivityConnections.FirstOrDefault(ac => ac.ActivitySourceId == activity.ActivityId);

                    if (connection == null)
                    {
                        return await PrepareFinalActivity(workflowInstance);
                    }

                    await UpdateState(workflowInstance, WorkflowStatus.RUNNING, workflowInstance.CurrentState, connection.ActivityIdTarget[Outcomes.DONE.ToString()]);
                    activity = workflowDefinitionData.Activities.Single(a => a.ActivityId == workflowInstance.CurrentActivityId);
                }

                // tuka veche vliza v while cikula, toest nqma nikvi neshta da vzima ot usera (ne mu trqq input poiche v sstatus running e, imame aktiviti vzeto otgore primerno red 276 ili 261)
                while (workflowInstance.Status == WorkflowStatus.RUNNING)
                {
                    if (activity.ActivityType == nameof(MiniWFActivity))
                    {
                        MiniWFActivity miniWFActivity = (MiniWFActivity)_activityService.CreateInstanceByActivityType<IActivity>(
                            activity
                            );

                        ActivityDefinition nextActivity = await _miniWfService.GetNextAcitivty(
                            miniWFActivity.WfDefinitionId,
                            workflowInstance.MiniWFStep,
                            workflowInstance.ActivityResults?.Last().Outcome);

                        if (nextActivity != null)
                        {
                            activity = nextActivity;
                            workflowInstance.MiniWFStep = nextActivity.ActivityId;

                        }
                        else
                        {
                            ActivityConnection? afterMiniWFConnection = workflowDefinitionData
                                .ActivityConnections
                                .FirstOrDefault(ac => ac.ActivitySourceId == activity.ActivityId);

                            workflowInstance.MiniWFStep = -2;

                            if (afterMiniWFConnection == null)
                            {
                                return await PrepareFinalActivity(workflowInstance);
                            }

                            activity = workflowDefinitionData.Activities.Find(a => a.ActivityId == afterMiniWFConnection.ActivityIdTarget[Outcomes.DONE.ToString()]);
                        }
                    }

                    activityResult = ExecuteActivity(workflowInstance, activity, executeProcessAction);
                    activityResult.ExecutedBy = executeProcessAction.UserEmail;
                    activityResult.ExecutionTime = DateTime.UtcNow;
                    activityResult.ActivitySource = workflowInstance.CurrentActivityId;

                    workflowInstance.ActivityResults.Add(activityResult);

                    if (activityResult.Outcome == Outcomes.AWAIT_EXECUTE.ToString()
                        || activityResult.Outcome == Outcomes.AWAIT_RESUME.ToString()
                        || activityResult.Outcome == Outcomes.ONHOLD.ToString())
                    {
                        //tuka veche sme napraili execute na aktivitito, obache sme v status koito predpolaga che ne trqbva da produljava napred workflow-a i go spirame do tuk,
                        //kato mu setva status na baza na rezultata ot aktivitito i ne vdiga stepa i sushto taka izliza ot while-a
                        await UpdateState(workflowInstance, Enum.Parse<WorkflowStatus>(activityResult.Outcome.ToString()), activityResult.ActivityState);
                        continue;
                    }

                    //tuka veche se predpolaga che tr se vzeme sledvashtoto aktiviti shtoto tva deto sme execute-nali toku shto e minalo (ne mu trqq input ot usera poiche)
                    ActivityConnection? connection = workflowDefinitionData
                        .ActivityConnections
                        .FirstOrDefault(ac => ac.ActivitySourceId == activity.ActivityId);

                    if (connection == null)
                    {
                        return await PrepareFinalActivity(workflowInstance);
                    }


                    //ne e posleden step, produljavame s while-a i vzimame sledvashtoto aktiviti
                    await UpdateState(workflowInstance, WorkflowStatus.RUNNING, activityResult.ActivityState, connection.ActivityIdTarget[activityResult.Outcome]);
                    activity = workflowDefinitionData.Activities.Single(a => a.ActivityId == workflowInstance.CurrentActivityId);
                }

                return (activityResult.ActivityState);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error {ex.Message}, " +
                    $"{nameof(ExecuteNextStep)} with params wfId: {workflowInstance.Id} and " +
                    $"action {executeProcessAction?.ActivityData?.ActionData}");

                throw ex;
            }
        }

        private async Task UpdateState(WorkflowInstance wfInstance,
            WorkflowStatus status,
            ActivityStateResult response,
            int? newActivityId = null)
        {
            try
            {
                if (await _miniWfService.IsMiniWFNotExecuting(wfInstance.MiniWFStep))
                {
                    wfInstance.CurrentActivityId = newActivityId.HasValue ? newActivityId.Value : wfInstance.CurrentActivityId;
                }

                wfInstance.Status = status;
                wfInstance.CurrentState = response;

                await _workflowInstanceService.UpdateAsync(wfInstance.Id, wfInstance);
                await _dipService.UpdateWorkflowInstanceRecord(wfInstance);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error {ex}, " +
                    $"{nameof(UpdateState)} crashed with {ex.Message}, with params wfId: {wfInstance.Id} and " +
                    $"response is {response.ScreenId}");

                throw ex;
            }
        }

        public async Task<WorkflowInstance> StartWorkflowAsync(string workflowDefinitionId, string user)
        {
            try
            {
                return await _instanceConfigurator.CreateInstanceAsync(workflowDefinitionId, user);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error {ex}, " +
                    $"{nameof(StartWorkflowAsync)} crashed with {ex.Message}, " +
                    $"with params defId: {workflowDefinitionId} and " +
                    $"user is {user}");

                throw ex;
            }
        }

        private ActivityResult ExecuteActivity(WorkflowInstance workflowInstance,
            ActivityDefinition activity,
            ExecuteProcessAction executeProcessAction)
        {
            var wfContext = new WorkflowContext(null, null, workflowInstance, executeProcessAction);

            ActivityResult activityResult = CreateActivityInstance(activity).Execute(wfContext);

            _logger.LogInformation($"Executing activity {workflowInstance.Id} with connection {activity.ActivityId} " + DateTime.Now);

            return activityResult;
        }


        private IActivity CreateActivityInstance(ActivityDefinition activity)
        {
            var activityInstance = _activityService.CreateInstanceByActivityType<IActivity>(activity);

            return activityInstance;
        }

        private ActivityResult ResumeActivity(WorkflowInstance workflowInstance, ActivityDefinition activity, ExecuteProcessAction executeProcessAction)
        {
            var wfContext = new WorkflowContext(null, null, workflowInstance, executeProcessAction);
            var activityResult = CreateActivityInstance(
                activity).Resume(wfContext);

            _logger.LogInformation($"Resuming activity {workflowInstance.Id} with connection {activity.ActivityId} " + DateTime.Now);
            return activityResult;
        }


        private async Task<ActivityStateResult> CompleteWorkflow(WorkflowInstance workflowInstance, string email)
        {
            var finalResult = new ActivityResult();

            finalResult.ActivityState = new ActivityStateResult();
            finalResult.ActivityState.Success = true;
            finalResult.ActivityState.Error = new Error { Code = 0, Message = "Workflow Complete" };
            await UpdateState(workflowInstance, WorkflowStatus.FINISHED, finalResult.ActivityState);

            UserSession userSession = await _userSessionService.GetAsync(email);
            await _userSessionService.ResetAsync(userSession.Id, userSession);

            return finalResult.ActivityState;

        }

        private async Task<ActivityStateResult> PrepareFinalActivity(WorkflowInstance workflowInstance)
        {
            ActivityStateResult activityStateResult = new ActivityStateResult();
            activityStateResult.ScreenId = "2b";
            activityStateResult.ActivityMetadata = "{\"UserMessage\": \"You have finished the workflow\"}";
            activityStateResult.ActivityType = "MessageScreenActivity";
            activityStateResult.Error = new Error();
            await UpdateState(workflowInstance, WorkflowStatus.FINAL_ACTIVITY, activityStateResult);
            return activityStateResult;
        }
    }
}
