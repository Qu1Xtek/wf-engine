using MongoDB.Bson.Serialization.Attributes;

namespace WorkflowConfigurator.Models.Materials
{
    public class Category
    {
        [BsonId]
        public string? Id { get; set; }
        public string? Name { get; set; }
    }
}
