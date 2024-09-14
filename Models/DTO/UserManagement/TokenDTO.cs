using Newtonsoft.Json;

namespace WorkflowConfigurator.Models.DTO.UserManagement
{
    public class TokenDTO
    {
        [JsonProperty("username")]
        public string UserName { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("company")]
        public string Company { get; set; }

        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("active")]
        public bool Active { get; set; }

        public bool IsNotActive => !Active;

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}
