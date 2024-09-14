namespace WorkflowConfigurator.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PossibleOutcomesAttribute : Attribute
    {
        private readonly string _outcomes;

        public PossibleOutcomesAttribute(string csvOutcomes)
        {
            _outcomes = csvOutcomes;
        }

        public override string ToString()
        {
            return _outcomes;
        }
    }
}
