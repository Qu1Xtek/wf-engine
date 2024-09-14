using MongoDB.Bson.Serialization.Attributes;

namespace WorkflowConfigurator.Models.Materials
{
    public class Material
    {
        [BsonId]
        public string? Id { get; set; }
        public string? Name { get; set; }

        public string? MaterialType { get; set; }

        public string? Image { get; set; }

    }
}
