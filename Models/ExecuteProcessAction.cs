
using System.Text.Json.Serialization;

namespace WorkflowConfiguration.Models
{
    public class ExecuteProcessAction
    {
        public int UserId { get; set; }
        public string? DeviceId { get; set; }
        public ActivityData? ActivityData { get; set; }

        [JsonIgnore]
        public string UserEmail { get; set; } = string.Empty;
    }
}