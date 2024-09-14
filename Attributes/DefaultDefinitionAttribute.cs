namespace WorkflowConfigurator.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DefaultDefinitionAttribute : Attribute
    {
        private readonly string _definition;

        public DefaultDefinitionAttribute(string definition)
        {
            _definition = definition;
        }

        public override string ToString()
        {
            return _definition;
        }
    }
}
