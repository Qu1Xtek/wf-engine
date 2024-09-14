namespace WorkflowConfigurator.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DataAddressAttribute : Attribute
    {
        private readonly string _value;

        public DataAddressAttribute(string urlValue)
        {
            _value = urlValue;
        }

        public override string ToString()
        {
            return _value;
        }
    }
}
