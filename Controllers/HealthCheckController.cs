using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkflowConfiguration.Infrastructure;
using WorkflowConfiguration.Models;

namespace WorkflowConfigurator.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("HealthCheck")]
    public class HealthCheckController : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public async Task<bool> HealthCheck()
        {
            return true;
        }
    }
}