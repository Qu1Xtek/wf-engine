namespace WorkflowConfigurator.Models.Workflow
{
    public enum WorkflowStatus
    {
        RUNNING,
        FAULT,
        PAUSE,
        AWAIT_EXECUTE,
        AWAIT_RESUME,
        ONHOLD,
        FINAL_ACTIVITY,
        FINISHED
    }
}