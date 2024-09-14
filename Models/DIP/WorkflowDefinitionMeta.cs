namespace WorkflowConfigurator.Models.DIP
{
    public class WorkflowDefinitionMeta
    {

        public WorkflowDefinitionMeta(string hash)
        {
            Hash = hash;
        }

        public string Hash { get; set; }
    }
}
