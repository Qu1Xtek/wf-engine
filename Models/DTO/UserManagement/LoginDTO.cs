using Newtonsoft.Json;

namespace WorkflowConfigurator.Models.DTO.UserManagement
{
    public class LoginDTO
    {
        public LoginDTO()
        {

        }

        public LoginDTO(string uname, string password)
        {
            Email = uname;
            Password = password;
        }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
