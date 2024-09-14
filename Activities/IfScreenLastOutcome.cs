using Newtonsoft.Json;
using WorkflowConfiguration.Interface;
using WorkflowConfigurator.Attributes;
using WorkflowConfigurator.Models.Activity;
using WorkflowConfigurator.Models.ActivityAtributeTypes;

namespace WorkflowConfiguration.Activities
{
    [PossibleOutcomes("FALSE, TRUE")]
    [DefaultDefinition("Activity that provides a split point based on configured Operator, Result from last step and configured Value to check against")]
    [ActivityTitle("Branching Choice")]
    public class IfScreenLastOutcome : Activity, IActivity
    {
        private Dictionary<string, Func<WorkflowContext, ActivityResult>> _Expressions;

        [TypeOverwrite(OverwrittenType.DROPDOWN)]
        [PredifinedValues("Equals, GreaterThan, LesserThan")]
        public string ConditionOperator { get; set; }

        public string ValueToCheckAgainst { get; set; }

        public IfScreenLastOutcome()
        {
            _Expressions = new Dictionary<string, Func<WorkflowContext, ActivityResult>>
            {
                { "Equals", Equals },
                { "GreaterThan", GreaterThan },
                { "LesserThan", LesserThan }
            };
        }

        public ActivityResult Execute(WorkflowContext workflowContext)
        {
            return _Expressions[ConditionOperator].Invoke(workflowContext);
        }

        private ActivityResult Equals(WorkflowContext context)
        {
            if (decimal.Parse(context.WfInstance.LastOutput) == decimal.Parse(ValueToCheckAgainst))
            {
                return Outcome("TRUE", GetResult("TRUE", context.WfInstance.LastOutput));
            }

            return Outcome("FALSE", GetResult("FALSE", context.WfInstance.LastOutput));
        }

        private ActivityResult GreaterThan(WorkflowContext context)
        {
            if (decimal.Parse(context.WfInstance.LastOutput) > decimal.Parse(ValueToCheckAgainst))
            {
                return Outcome("TRUE", GetResult("TRUE", context.WfInstance.LastOutput));
            }

            return Outcome("FALSE", GetResult("FALSE", context.WfInstance.LastOutput));
        }

        private ActivityResult LesserThan(WorkflowContext context)
        {
            if (decimal.Parse(context.WfInstance.LastOutput) < decimal.Parse(ValueToCheckAgainst))
            {
                return Outcome("TRUE", GetResult("TRUE", context.WfInstance.LastOutput));
            }

            return Outcome("FALSE", GetResult("FALSE", context.WfInstance.LastOutput));
        }

        private ActivityResult GetResult(string resultOutput, string lastOutput)
        {
            return new ActivityResult()
            {
                ResultOutput = resultOutput,
                ActivityState = new ActivityStateResult()
                {
                    Error = new Error { Code = 0, Message = "No error" },
                    Success = true,
                    ActivityMetadata = JsonConvert.SerializeObject(this),
                    ActivityType = nameof(IfScreenLastOutcome),
                    Input = lastOutput // Last output of the previous activity is the input for this "IF"
                }
            };
        }
    }
}
