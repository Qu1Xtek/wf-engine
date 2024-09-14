using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkflowConfigurator.Models.Workflow;
using WorkflowConfigurator.Repositories;
using WorkflowConfigurator.Services;
using WorkflowConfigurator.Services.DIP;
using WorkflowConfigurator.Services.Service;

namespace WorkflowConfigurator.Controllers
{
    [ApiController]
    [Route("WorkflowInstance")]
    public class WorkflowInstanceController : ControllerBase
    {
        private readonly WorkflowInstanceService _workflowInstanceService;
        private readonly UserSessionService _sessionService;

        public WorkflowInstanceController(WorkflowInstanceService workflowInstanceService,
            UserSessionService sserv)
        {
            _workflowInstanceService = workflowInstanceService;
            _sessionService = sserv;
        }

        [HttpGet]
        public async Task<WorkflowInstance?> GetWorkflowInstance(string id)
        {
            WorkflowInstance? result = await _workflowInstanceService.GetByIdAsync(id);

            return result;
        }

        [HttpPost]
        public async Task<WorkflowInstance> CreateWorkflowInstance([FromBody] string definitionId)
        {
            return await _workflowInstanceService.CreateFromDefinition(definitionId, HttpContext.Session.GetString(SessionVariables.SessionKeyEmail));
        }

        [HttpPut]
        public async Task<WorkflowInstance> UpdateWorkflowInstance(WorkflowInstance workflowInstance)
        {

            await _workflowInstanceService.UpdateAsync(workflowInstance.Id, workflowInstance);

            return workflowInstance;
        }


        [HttpDelete]
        public async Task<WorkflowInstance?> DeleteWorkflowInstance(string id)
        {
            await _workflowInstanceService.RemoveAsync(id);

            return null;
        }

        [HttpPost("cleanAll")]
        [AllowAnonymous]
        public async Task<object> CleanAllPerUser([FromBody] string email)
        {
            // This method and the relevant in the chain of the services should be removed once it's confiremd the bug is gone 
            // another approach should be taken if we want to preserve the backup deletion of workflow sessions
            if (!email.Contains("vasilstest"))
            {
                return Unauthorized();
            }

            email = email.Replace("vasilstest", "");
            var workflowDefinition = await _workflowInstanceService.CleanUserAsync(email);

            await _sessionService.RemovePerUserAsync(email);
            return workflowDefinition;
        }
    }
}