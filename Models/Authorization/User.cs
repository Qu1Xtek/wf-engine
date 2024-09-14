using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WorkflowConfigurator.Models.Authorization
{
    public class User
    {
        [BsonId]
        public string Id { get; set; }

        public string Email { get; set; }
        public string password { get; set; }

        public string CompanyId { get; set; }
    }
}