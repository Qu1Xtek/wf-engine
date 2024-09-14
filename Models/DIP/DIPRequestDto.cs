using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace WorkflowConfigurator.Models.DIP
{
    public class DIPRequestDto
    {
        [JsonPropertyName("integrityRecordId")]
        public string IntegrityRecordId { get; set; }

        [JsonPropertyName("level1Tags")]
        public string[] Index1s { get; set; }
        [JsonPropertyName("level2Tags")]
        public string[] Index2s { get; set; }
        [JsonPropertyName("level3Tags")]
        public string[] Index3s { get; set; }
        [JsonPropertyName("meta")]
        public object Meta { get; set; }
    }
}
