
using NCalc;
using Newtonsoft.Json;
using WorkflowConfiguration.Interface;
using WorkflowConfigurator.Attributes;
using WorkflowConfigurator.Models.Activity;

namespace WorkflowConfigurator.Activities
{
    [DefaultDefinition("Activity used to calculate based on selected Operator, Result from last step and Value selected during configuration")]
    [ActivityTitle("Calculation")]
    public class CalculationActivity : Activity, IActivity
    {
        private Dictionary<string, Func<WorkflowContext, ActivityResult>> _Expressions;

        [DescriptionProp("Formula to apply to last activity output (X) e.g. (X+1)/3")]
        public string Formula { get; set; }

        public CalculationActivity()
        {

        }

        public ActivityResult Execute(WorkflowContext workflowContext)
        {
            var valueForX = workflowContext.WfInstance.LastOutput;
            var execFormula = Formula.Replace("X", valueForX);

            var expression = new Expression(execFormula);
            var result = expression.Evaluate().ToString();

            var ar = new ActivityResult
            {
                ResultOutput = result,
                ActivityState = new ActivityStateResult()
                {
                    Error = new Error { Code = 0, Message = "No error" },
                    Success = true,
                    ActivityMetadata = JsonConvert.SerializeObject(this),
                    ActivityType = nameof(CalculationActivity),
                    Input = Formula
                }
            };

            workflowContext.WfInstance.SetLastOutput(result);
            workflowContext.WfInstance.GlobalVariables["LastCalculationResult"] = result;
            return BaseDoneResponse(valueForX, result);
        }

        private ActivityResult BaseDoneResponse(string calculationInput, string calculationResult)
        {
            var result = new ActivityResult()
            {
                ResultOutput = calculationResult,
                ActivityState = new ActivityStateResult()
                {
                    Error = new Error { Code = 0, Message = "No error" },
                    Success = true,
                    ActivityMetadata = JsonConvert.SerializeObject(this),
                    ActivityType = nameof(CalculationActivity),
                    Input = calculationInput.ToString()
                }
            };

            return Done(result);
        }
    }
}
