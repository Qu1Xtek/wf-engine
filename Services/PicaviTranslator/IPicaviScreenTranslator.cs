using WorkflowConfiguration.Interface;
using WorkflowConfigurator.Models.Activity;

namespace WorkflowConfigurator.Services.PicaviTranslator
{
    public interface IPicaviScreenTranslator
    {
        PicaviResponse BuildExecuteResponse(ActivityStateResult activityStateResult);

        PicaviResponse BuildResumeResponse(ActivityStateResult activityStateResult);

        Task<PicaviResponse> BuildOnholdResponse(string workflowInstanceId, ActivityStateResult activityStateResult);
    }
}
