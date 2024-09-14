namespace WorkflowConfigurator.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class TypeOverwriteAttribute : Attribute
    {
        private readonly string _typeName;

        public TypeOverwriteAttribute(string typeName)
        {
            _typeName = typeName;
        }

        public override string ToString()
        {
            return _typeName;
        }
    }
}
