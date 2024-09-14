using System.Text.Json.Serialization;

namespace WorkflowConfiguration.Models
{
    public class ActivityData
    {
        public string? ActionData { get; set; }
        public string? ScannedValue { get; set; }
        public string? Input { get; set; }
        public string? Hotkey { get; set; }
        public string? Action { get; set; }

        [JsonPropertyName("Confirm")]
        public string? Confirm { get; set; }
    }
}