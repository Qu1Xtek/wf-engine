using WorkflowConfigurator.Models.Authorization;
using WorkflowConfigurator.Repositories;

namespace WorkflowConfigurator.Services
{
    public class LoginService
    {
        private readonly WebService _webService;
        private readonly IJWTManagerRepository _jWTManager;
        private readonly UserService _userService;

        public LoginService(WebService webService, IJWTManagerRepository jWTManager, UserService service)
        {
            _webService = webService;
            _jWTManager = jWTManager;
            _userService = service;
        }

        public async Task<LoginResponse> Login(string username, string password, string deviceId)
        {
            var token = await _jWTManager.Authenticate(username, password);

            if (token == null || token.IsNotActive)
            {
                return null;
            }

            ISession session = _webService.CreateLoginSession(token.Email, deviceId, token.Company);

            WebService.UserLoginSessions[token.Email] = token.AccessToken;

            return new LoginResponse() 
            {
                Token = token.AccessToken,
                SessionId = session.Id
            };
        }
        
        public bool Logoff()
        {
            _webService.LogoffAndRemoveSession();

            return true;
        }
    }
}
