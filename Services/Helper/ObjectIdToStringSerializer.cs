using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace WorkflowConfigurator.Services.Helper
{
    public class ObjectIdToStringSerializer : SerializerBase<string>
    {
        public override string Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;
            var objectId = bsonReader.ReadObjectId();
            return objectId.ToString();
        }

        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, string value)
        {
            var bsonWriter = context.Writer;
            if (ObjectId.TryParse(value, out var objectId))
            {
                bsonWriter.WriteObjectId(objectId);
            }
            else
            {
                bsonWriter.WriteObjectId(ObjectId.Empty);
            }
        }
    }
}
