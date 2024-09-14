using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkflowConfiguration.Models;
using WorkflowConfigurator.Models.Activity;
using WorkflowConfigurator.Models.DTO.Workflow;
using WorkflowConfigurator.Services.Service;


namespace WorkflowConfigurator.Controllers
{
    [ApiController]
    [Route("WorkflowInvoker")]
    public class WorkflowInvokerController : ControllerBase
    {
        private readonly WorkflowInvokerService _workflowInvokerService;

        public WorkflowInvokerController(WorkflowInvokerService workflowInvokerService)
        {
            _workflowInvokerService = workflowInvokerService;
        }

        [HttpPost, Route("Progress")]
        public async Task<object> ProgressWorkflow([FromBody] ExecuteProcessAction executeProcessAction)
        {

            ServiceResponse response = await _workflowInvokerService.ProgressWorkflow(executeProcessAction);

            if (response.ErrorCode == 200)
            {
                return Ok(response.Message);
            }
            else if (response.ErrorCode == 400)
            {
                return BadRequest(response.Message);

            } else if (response.ErrorCode == 500)
            {
                return StatusCode(response.ErrorCode, response.Message);
            }
            else
            {
                return response.ResponseObject;
            }
        }

        [HttpGet]
        public async Task<PicaviResponse> GetWorkflowState()
        {
            return await _workflowInvokerService.GetState();
        }

        [HttpPost, Route("addScan")]
        [AllowAnonymous]
        public async Task<string> InjectScan([FromBody] RunningWFScan data)
        {
            return await _workflowInvokerService.InjectScan(data);
        }


    }
}
