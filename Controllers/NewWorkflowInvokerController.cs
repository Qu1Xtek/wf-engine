using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkflowConfiguration.Models;
using WorkflowConfigurator.Models;
using WorkflowConfigurator.Models.Activity;
using WorkflowConfigurator.Models.DTO.Workflow;
using WorkflowConfigurator.Models.Response;
using WorkflowConfigurator.Services.Service;


namespace WorkflowConfigurator.Controllers
{
    [ApiController]
    [Route("NewWorkflowInvoker")]
    public class NewWorkflowInvokerController : ControllerBase
    {
        private readonly WorkflowInvokerService _workflowInvokerService;

        public NewWorkflowInvokerController(WorkflowInvokerService workflowInvokerService)
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
        public async Task<GRes<WorkflowInvokerResponse>> GetWorkflowState()
        {
            var response = new WorkflowInvokerResponse(await _workflowInvokerService.NewGetState());

            return GRes<WorkflowInvokerResponse>.OK(response);
        }

        [HttpPost, Route("addScan")]
        [AllowAnonymous]
        public async Task<string> InjectScan([FromBody] RunningWFScan data)
        {
            return await _workflowInvokerService.InjectScan(data);
        }


    }
}
