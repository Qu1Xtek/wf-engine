namespace WorkflowConfigurator.Repositories
{
    public class SessionVariables
    {
        public const string SessionKeyEmail = "SessionKeyUsername";
        public const string SessionKeyDeviceId = "SessionKeyDeviceId";
        public const string SessionKeySessionId = "SessionKeySessionId";
        public const string SessionKeyCompanyId = "SessionKeyCompanyId";

        public const string SessionKeyWorkflowInstance = "SessionKeyWorkflowInstance";
        public const string SessionKeyWorkflowDefinition = "SessionKeyWorkflowDefinition";
        public const string SessionKeyWorkflowInstancesList = "SessionKeyWorkflowInstancesList";
        public const string SessionKeyWorkflowDefinitionsList = "SessionKeyWorkflowDefinitionsList";
    }


    public enum SessionKeyEnum
    {
        SessionKeyUsername = 0,
        SessionKeyDeviceId = 1,
        SessionKeySessionId = 2,
        SessionKeyWorkflowInstance = 3,
        SessionKeyWorkflowDefinition = 4,
        SessionKeyWorkflowInstancesList = 5,
        SessionKeyWorkflowDefinitionsList = 6
    }
}
