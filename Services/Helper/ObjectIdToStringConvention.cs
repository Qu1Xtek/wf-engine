using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace WorkflowConfigurator.Services.Helper
{
    public class ObjectIdToStringConvention : ConventionBase, IMemberMapConvention
    {
        public void Apply(BsonMemberMap memberMap)
        {
            if (memberMap.MemberName == "Id")
            {
                memberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
                memberMap.SetIdGenerator(new CustomObjectIdGenerator());
            }
        }
    }
}
