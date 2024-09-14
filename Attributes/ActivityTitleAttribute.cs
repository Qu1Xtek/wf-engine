namespace WorkflowConfigurator.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ActivityTitleAttribute : Attribute
    {
        private readonly string _title;

        public ActivityTitleAttribute(string title)
        {
            _title = title;
        }

        public override string ToString()
        {
            return _title;
        }
    }
}
