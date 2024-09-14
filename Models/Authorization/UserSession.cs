using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using WorkflowConfigurator.Services;

namespace WorkflowConfigurator.Models.Authorization
{
    public class UserSession
    {
        [BsonId]
        public string Id { get; set; }
        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("deviceId")]
        public string DeviceId { get; set; }

        public string WorkflowInstanceId { get; set; }

        public string WorkflowDefinitionId { get; set; }
    }
}