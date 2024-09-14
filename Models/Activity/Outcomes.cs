namespace WorkflowConfigurator.Models.Activity
{
    public enum Outcomes
    {
        DONE,
        FAULT,
        AWAIT_EXECUTE,
        AWAIT_RESUME,
        ONHOLD,
        FINAL_ACTIVITY,
        WF_COMPLETED
    }
}
