using WorkflowConfigurator.Models.Workflow;
using WorkflowConfigurator.Models.Workflow.Dto;

namespace WorkflowConfigurator.Interface.Workflow
{
    public interface IWorkflowReporter
    {
        /// <summary>
        /// Returns the last X instances basic report
        /// (BASIC - Only instance ID's are returned)
        /// </summary>
        /// <returns></returns>
        Task<List<WorkflowReportDTO>> GetRecentReport(string companyId);

        /// <summary>
        /// Get all workflow instances report for specific period of time
        /// (BASIC - Only instance ID's are returned)
        /// </summary>
        /// <returns></returns>
        Task<List<WorkflowReportDTO>> GetReportsBetween(DateTime from, DateTime to, string companyId);

        /// <summary>
        /// Get workflow report per specific workflow instance
        /// </summary>
        /// <param name="wfId">Id of a workflow instance</param>
        /// <returns></returns>
        Task<WorkflowReportDTO> GetReportById(string wfId);
    }
}
