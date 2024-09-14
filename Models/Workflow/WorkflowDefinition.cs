
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;
using WorkflowConfigurator.Models.Workflow.Dto;

namespace WorkflowConfigurator.Models.Workflow
{
    public class WorkflowDefinition
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        [BsonElement("_id")]
        public string Id { get; set; }
        public string? Name { get; set; }
        public object? Context { get; set; }
        public int Version { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string? Creator { get; set; }
        public string? Reviewer { get; set; }
        public string? CompanyId { get; set; }
        public bool IsMiniWorkflow { get; set; }

        public WorkflowDefinitionDto? Draft { get; set; }
        public DateTime? DraftLastUpdateDate { get; set; }
        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        public Dictionary<int, WorkflowDefinitionData> WorkflowDefinitionVersionData { get; set; }

    }

}
