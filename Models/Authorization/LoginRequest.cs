using System.Text.Json.Serialization;

namespace WorkflowConfigurator.Models.Authorization
{
    public class LoginRequest
    {
        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("deviceId")]
        public string DeviceId { get; set; }
    }
}