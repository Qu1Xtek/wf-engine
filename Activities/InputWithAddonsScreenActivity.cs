using Newtonsoft.Json;
using WorkflowConfiguration.Interface;
using WorkflowConfiguration.Models;
using WorkflowConfigurator.Attributes;
using WorkflowConfigurator.Models;
using WorkflowConfigurator.Models.Activity;

namespace WorkflowConfiguration.Activities
{
    [PossibleOutcomes("TRUE, FALSE")]
    [DefaultDefinition("Activity used for User input, which can also have Addons - " +
        "Calculation Formula which will be used for performing mathematical action based " +
        "on Input and Result from last step, Conditional Formula which will be used as" +
        " a split point based on Result from Calculation and Value configured in the formula itself")]
    
    [ActivityTitle("Input with Integrated Processes")]
    public class InputWithAddonsScreenActivity : Activity, IActivity
    {
        public string UserMessage { get; set; }

        public string? Formula { get; set; }

        public string? ConditionalFormula { get; set; }

        private Dictionary<char, Func<decimal, decimal, decimal>> _mathematicalExpressions;
        private Dictionary<char, Func<decimal, decimal, bool>> _logicalExpressions;

        public InputWithAddonsScreenActivity()
        {
            _mathematicalExpressions = new Dictionary<char, Func<decimal, decimal, decimal>>
            {
                { '+', Add },
                { '-', Substract },
                { '*', Multiply },
                { '/', Divide },
            };

            _logicalExpressions = new Dictionary<char, Func<decimal, decimal, bool>>
            {
                { '>', GreaterThan },
                { '=', LogicalEquals },
                { '<', LessThan },
            };
        }

        public ActivityResult Execute(WorkflowContext workflowContext)
        {
            var result = new ActivityResult()
            {
                ActivityState = new ActivityStateResult()
                {
                    ScreenId = "123",
                    Error = new Error { Code = 0, Message = "No error" },
                    Success = true,
                    ActivityMetadata = JsonConvert.SerializeObject(this),
                    ActivityType = nameof(InputWithAddonsScreenActivity)
                }
            };

            return Outcome(Outcomes.AWAIT_EXECUTE.ToString(), result);
        }

        public override ActivityResult Resume(WorkflowContext workflowContext)
        {
            var result = decimal.Parse(workflowContext.ProcessAction.ActivityData.Input);

            if (!String.IsNullOrEmpty(Formula))
            {
                var lastOutput = decimal.Parse(workflowContext.WfInstance.LastOutput);
                result = ApplyFormula(result, lastOutput, Formula);
            }

            if (!String.IsNullOrEmpty(ConditionalFormula))
            {
                var boolResult = ApplyConditionalFormula(result, ConditionalFormula);

                string stringBool = boolResult.ToString();
                workflowContext.WfInstance.SetLastOutput(stringBool);

                return Outcome(stringBool.ToUpper(), GenerateResultObject(stringBool));
            }

            workflowContext.WfInstance.SetLastOutput(result.ToString());

            return Outcome("TRUE", GenerateResultObject("TRUE"));
        }


        private ActivityResult GenerateResultObject(string outcome)
        {
            return new ActivityResult
            {
                ResultOutput = outcome,
                ActivityState = new ActivityStateResult
                {
                    ActivityMetadata = JsonConvert.SerializeObject(this)
                }
            };
        }
        private decimal ApplyFormula(decimal input, decimal lastOutput, string formula)
        {
            char mathematicalOperator = formula[1];

            return _mathematicalExpressions[mathematicalOperator].Invoke(lastOutput, input);
        }

        private bool ApplyConditionalFormula(decimal input, string formula)
        {
            char logicalOperator = formula[1];
            decimal valueToCheckAgainst = decimal.Parse(formula.Substring(2));

            return _logicalExpressions[logicalOperator].Invoke(input, valueToCheckAgainst);
        }

        private decimal Substract(decimal leftSide, decimal rightSide)
        {
            return leftSide - rightSide;
        }

        private decimal Add(decimal leftSide, decimal rightSide)
        {
            return leftSide + rightSide;
        }

        private decimal Divide(decimal leftSide, decimal rightSide)
        {
            return leftSide / rightSide;
        }

        private decimal Multiply(decimal leftSide, decimal rightSide)
        {
            return leftSide * rightSide;
        }

        private bool GreaterThan(decimal leftSide, decimal rightSide)
        {
            return (leftSide > rightSide);
        }

        private bool GreaterThanOrEquals(decimal leftSide, decimal rightSide)
        {
            return (leftSide >= rightSide);
        }

        private bool LogicalEquals(decimal leftSide, decimal rightSide)
        {
            return (leftSide == rightSide);
        }
        private bool LessThanOrEquals(decimal leftSide, decimal rightSide)
        {
            return (leftSide <= rightSide);
        }

        private bool LessThan(decimal leftSide, decimal rightSide)
        {
            return (leftSide < rightSide);
        }
    }
}
