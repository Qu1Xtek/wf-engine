using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkflowConfigurator.Models.DTO.UserManagement;
using WorkflowConfigurator.Services.Account;

namespace WorkflowConfigurator.Controllers.Accounts
{
    [ApiController]
    [Route("AccountManagagment")]
    [AllowAnonymous]
    public class AccountManagerController : ControllerBase
    {
        private readonly AccountMGTMService _accMgmtService;

        public AccountManagerController(AccountMGTMService mgmService)
        {
            _accMgmtService = mgmService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] AccountManagementDTO createDto)
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString();
            if (token != Environment.GetEnvironmentVariable("USER_MGMT_TOKEN"))
            {
                return Unauthorized();
            }

            return Ok(await _accMgmtService.CreateUser(createDto, token));
        }
    }
}
