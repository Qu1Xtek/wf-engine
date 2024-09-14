using Amazon.CognitoIdentity.Model;
using Microsoft.AspNetCore.Mvc;
using WorkflowConfigurator.Models.Activity;
using WorkflowConfigurator.Models.Workflow.Dto.ActivitiesDTOs;
using WorkflowConfigurator.Services;
using WorkflowConfigurator.Services.ActivityDefinitions;

namespace WorkflowConfigurator.Controllers
{
    [ApiController]
    [Route("Activities")]
    public class ActivitiesController : ControllerBase
    {
        private readonly WebService _webSErvice;
        private readonly ActivityDefinitionMaintainer _adr;

        private static List<ActivityTemplate> _activityTemplates;
        private readonly ActivityService _activityService;

        public ActivitiesController(
            ActivityDefinitionMaintainer activityDefRetriever, 
            WebService wbService, 
            ActivityService activityService)
        {
            _webSErvice = wbService;
            _adr = activityDefRetriever;
            _activityService = activityService;
        }

        [HttpGet]
        public async Task<List<ActivityTemplate>> GetActivities()
        {
            return await _adr.GetAllForCompany(_webSErvice.UserLogin.CompanyId);
        }

        [HttpGet]
        [Route("all")]
        public async Task<List<ActivityTemplate>> GetAll()
        {
            return await _adr.GetAll();
        }

        [HttpGet]
        [Route("companies")]
        public async Task<List<string>> GetCompanies()
        {
            if (_webSErvice.UserLogin.CompanyId.ToLowerInvariant() != "arxum")
            {
                // This should be moved into a validation with roles instead company
                throw new Exception("Something went completely wrong");
            }
            
            return await _adr.GetMappedCompanies();
        }

        [HttpGet]
        [Route("all/{companyId}")]
        public async Task<List<ActivityTemplate>> GetForCompany([FromRoute(Name = "companyId")] string companyId)
        {
            return await _adr.GetAllForCompany(companyId);
        }

        [HttpPost]
        public async Task<ActionResult<object>> UpdateMap(CompanyTemplateMap map)
        {
            if (_webSErvice.UserLogin.CompanyId.ToLowerInvariant() != "arxum")
            {
                // This should be moved into a validation with roles instead company
                throw new Exception("Something went completely wrong");
            }

            return await _adr.MapTempaltes(map.CompanyId, map.TemplateTypes);
        }            
    }
}