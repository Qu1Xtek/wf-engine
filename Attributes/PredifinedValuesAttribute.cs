namespace WorkflowConfigurator.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PredifinedValuesAttribute : Attribute
    {
        private readonly string _values;

        public PredifinedValuesAttribute(string csvValues)
        {
            _values = csvValues;
        }

        public override string ToString()
        {
            return _values;
        }
    }
}
