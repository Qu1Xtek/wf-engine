using Microsoft.AspNetCore.Mvc;
using System.Data;
using WorkflowConfigurator.Models.Authorization;
using WorkflowConfigurator.Models.Workflow;
using WorkflowConfigurator.Models.Workflow.Dto;
using WorkflowConfigurator.Repositories;
using WorkflowConfigurator.Services;
using WorkflowConfigurator.Services.DIP;
using WorkflowConfigurator.Services.Service;

namespace WorkflowConfigurator.Controllers
{
    [ApiController]
    [Route("WorkflowDefinition")]
    public class WorkflowDefinitionController : ControllerBase
    {
        private readonly WorkflowDefinitionService _workflowDefinitionService;
        private readonly UserService _userService;
        private readonly WebService _webService;


        public WorkflowDefinitionController(UserService userService,
            WorkflowDefinitionService workflowDefinitionService, 
            WebService webService)
        {
            _workflowDefinitionService = workflowDefinitionService;
            _userService = userService;
            _webService = webService; 
        }

        [HttpGet]
        public async Task<List<WorkflowDefinitionDto>> GetWorkflowDefition([FromQuery(Name = "onlyMini")] bool? onlyMinies)
        {
            var user = _webService.UserLogin;

            return await _workflowDefinitionService.GetWorkflowDefition(user, onlyMinies);
        }

        [HttpPost]
        public async Task<WorkflowDefinitionDto> CreateWorkflowDefition(WorkflowDefinitionDto workflowDefinitionDto)
        {
            return await _workflowDefinitionService.CreateAsync(workflowDefinitionDto, _webService.UserLogin);
        }

        [HttpPut]
        public async Task<WorkflowDefinitionDto> UpdateWorkflowDefition(WorkflowDefinitionDto workflowDefinitionDto)
        {
            return await _workflowDefinitionService.UpdateAsync(workflowDefinitionDto);
        }


        [HttpDelete]
        public async Task<object> DeleteWorkflowDefition(string id)
        {

            await _workflowDefinitionService.RemoveAsync(id);

            return Ok();
        }

        [HttpPost("SaveDraft")]
        public async Task<WorkflowDefinitionDto> SaveDraftWorkflow(WorkflowDefinitionDto workflowDefinitionDto)
        {
            try
            {
                return await _workflowDefinitionService.SaveDraftAsync(workflowDefinitionDto, _webService.UserLogin);
            }
            catch (Exception ex)
            {
                var requestAsJson = System.Text.Json.JsonSerializer.Serialize(workflowDefinitionDto, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

                Console.WriteLine($"Error {ex}, " +
                                       $"{nameof(SaveDraftWorkflow)} crashed with {ex.Message}, with params wfId: {workflowDefinitionDto.Id} and request params: {requestAsJson}");

                throw ex;
            }
        }

        [HttpGet("GetWorkflowByName/{name}")]
        public async Task<bool> GetWorkflowByName(string name)
        {
            var res = await _workflowDefinitionService.GetByNameAsync(name);

            if (res != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }
}