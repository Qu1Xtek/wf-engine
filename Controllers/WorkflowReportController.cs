using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using WorkflowConfigurator.Interface.Workflow;
using WorkflowConfigurator.Models.Workflow.Dto;

namespace WorkflowConfigurator.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("WorkflowReport")]
    public class WorkflowReportController : ControllerBase
    {
        private readonly IWorkflowReporter _workflowReportService;

        public WorkflowReportController(IWorkflowReporter reporter)
        {
            _workflowReportService = reporter;
        }

        [HttpGet]
        [Route("Recent")]
        [AllowAnonymous]
        public async Task<List<WorkflowReportDTO>> GetRecent()
        {
            var jwtToken = Request.Headers.Authorization
                .ToString()
                .Replace("Bearer", "")
                .Trim();

            var parsedJWTToken = new JwtSecurityTokenHandler().ReadJwtToken(jwtToken);
            var companyId = parsedJWTToken.Claims.First(x => x.Type == "cognito:groups").Value;

            var wfReport = await _workflowReportService.GetRecentReport(companyId);
            return wfReport;
        }

        [HttpGet]
        [Route("InBetween")]
        [AllowAnonymous]
        public async Task<List<WorkflowReportDTO>> InBetween([FromQuery] DateTime dateFrom, [FromQuery] DateTime dateTo)
        {
            var jwtToken = Request.Headers.Authorization
                .ToString()
                .Replace("Bearer", "")
                .Trim();

            var parsedJWTToken = new JwtSecurityTokenHandler().ReadJwtToken(jwtToken);
            var companyId = parsedJWTToken.Claims.First(x => x.Type == "cognito:groups").Value;

            var wfReport = await _workflowReportService.GetReportsBetween(dateFrom, dateTo, companyId);

            return wfReport;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<WorkflowReportDTO> WorkflowReport([FromQuery(Name = "instanceId")] string instanceId)
        {
            return await _workflowReportService.GetReportById(instanceId);
        }
    }
}