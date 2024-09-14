using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace WorkflowConfigurator.Models
{
    public class ProjectSettings
    {
        [BsonId]
        public string Id { get; set; }

        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<string, string[]> CompanyActivities { get; set; }
    }
}
