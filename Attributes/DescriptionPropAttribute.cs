namespace WorkflowConfigurator.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DescriptionPropAttribute : Attribute
    {
        private readonly string _value;

        public DescriptionPropAttribute(string description)
        {
            _value = description;
        }

        public override string ToString()
        {
            return _value;
        }
    }
}
