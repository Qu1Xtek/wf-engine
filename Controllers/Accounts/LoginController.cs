using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkflowConfigurator.Models.Authorization;
using WorkflowConfigurator.Repositories;
using WorkflowConfigurator.Services;

namespace WorkflowConfigurator.Controllers.Accounts
{
    [ApiController]
    [Route("Login")]
    public class LoginController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly UserSessionService _userSessionService;
        private readonly LoginService _loginService;
        public LoginController(UserService userService, UserSessionService userSessionService, LoginService loginService)
        {
            _userService = userService;
            _userSessionService = userSessionService;
            _loginService = loginService;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<object> Login([FromBody] LoginRequest loginRequest)
        {
            var loginResponse = await _loginService.Login(loginRequest.Username,
                loginRequest.Password,
                loginRequest.DeviceId);

            if (loginResponse == null)
            {
                return Unauthorized();
            }

            Response.Cookies.Append("JWT-Access-Token", loginResponse.Token, new CookieOptions()
            {
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true,
                Expires = DateTimeOffset.UtcNow.AddHours(24)
            });


            return loginResponse.Token;
        }

        [HttpPost]
        [Route("Logoff")]
        public async Task<object> Logoff()
        {
            return _loginService.Logoff();
        }


    }
}