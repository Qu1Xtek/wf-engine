using MongoDB.Bson.Serialization.Attributes;
using WorkflowConfigurator.Services.Helper;

namespace WorkflowConfigurator.Models
{
    public class ActivityTimer
    {
        [BsonId]
        public string Id { get; set; }

        public DateTime ActiveUntil { get; set; }
        public string WorkflowInstanceId { get; set; }
    }
}
