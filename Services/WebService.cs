using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using WorkflowConfigurator.Repositories;

namespace WorkflowConfigurator.Services
{
    public class WebService
    {

        private readonly IHttpContextAccessor _accessor;
        private readonly IJWTManagerRepository _jwtRepo;
        static public readonly Dictionary<string, string> UserLoginSessions = new Dictionary<string, string>();

        public UserLogin UserLogin => GetUserLogin();

        public WebService(IHttpContextAccessor accessor, IJWTManagerRepository jwtRepo)
        {
            _accessor = accessor;
            _jwtRepo = jwtRepo;


        }


        private UserLogin GetUserLogin()
        {
            return new UserLogin(
                _accessor.HttpContext.Session.GetString(SessionVariables.SessionKeyEmail),
                _accessor.HttpContext.Session.GetString(SessionVariables.SessionKeyDeviceId),
                _accessor.HttpContext.Session.GetString(SessionVariables.SessionKeyCompanyId)
                );
        }

        public async Task<bool> ValidateSession(MessageReceivedContext context)
        {
            string username = _accessor.HttpContext.Session.GetString(SessionVariables.SessionKeyEmail);
            bool isNotAnonymous = !(_accessor.HttpContext.GetEndpoint()?.Metadata?.GetMetadata<IAllowAnonymous>() is object);

            context.Token = context.Request.Cookies["JWT-Access-Token"];

            if (isNotAnonymous && (username == null 
                //|| await _jwtRepo.IsInvalid(context.Token)
                ))
            {
                _accessor.HttpContext.Response.StatusCode = 401;
                _accessor.HttpContext.Response.Cookies.Delete("JWT-Access-Token");
                _accessor.HttpContext.Response.Cookies.Delete(".AspNetCore.Session");

                return false;
            }

            if (isNotAnonymous && (username == "" || context.Token != UserLoginSessions[username]))
            {
                _accessor.HttpContext.Response.Cookies.Delete("JWT-Access-Token");
                _accessor.HttpContext.Response.Cookies.Delete(".AspNetCore.Session");
                _accessor.HttpContext.Response.StatusCode = 401;

                return false;
            }

            return true;
        }

        public ISession CreateLoginSession(string email, string deviceId, string companyId)
        {
            _accessor.HttpContext.Session.SetString(SessionVariables.SessionKeyEmail, email);
            _accessor.HttpContext.Session.SetString(SessionVariables.SessionKeyDeviceId, deviceId);
            _accessor.HttpContext.Session.SetString(SessionVariables.SessionKeyCompanyId, companyId);
            _accessor.HttpContext.Session.SetString(SessionVariables.SessionKeySessionId, Guid.NewGuid().ToString());

            return _accessor.HttpContext.Session;
        }

        public void LogoffAndRemoveSession()
        {
            _accessor.HttpContext.Response.Cookies.Delete("JWT-Access-Token");
            _accessor.HttpContext.Response.Cookies.Delete(".AspNetCore.Session");
            _accessor.HttpContext.Session.SetString(SessionVariables.SessionKeyEmail, "");
            _accessor.HttpContext.Session.SetString(SessionVariables.SessionKeyDeviceId, "");
            _accessor.HttpContext.Session.SetString(SessionVariables.SessionKeySessionId, "");
        }
    }

    /// <summary>
    /// Class used to store in memory essential current user data 
    /// </summary>
    public class UserLogin
    {
        public string Email { get; }
        public string DeviceId { get; }
        public string CompanyId { get; }

        public UserLogin(string email, string deviceId, string companyId)
        {
            Email = email;
            DeviceId = deviceId;
            CompanyId = companyId;
        }
    }
}
