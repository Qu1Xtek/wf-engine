using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace WorkflowConfigurator.Services.Helper
{
    public class ListSerializer<T> : SerializerBase<List<T>>
    {
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, List<T> value)
        {
            context.Writer.WriteStartArray();
            foreach (var item in value)
            {
                context.Writer.WriteStartDocument();
                BsonSerializer.Serialize(context.Writer, typeof(T), item);
                context.Writer.WriteEndDocument();
            }
            context.Writer.WriteEndArray();
        }

        public override List<T> Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var list = new List<T>();
            context.Reader.ReadStartArray();
            while (context.Reader.ReadBsonType() != BsonType.EndOfDocument)
            {
                context.Reader.ReadStartDocument();
                var item = BsonSerializer.Deserialize<T>(context.Reader);
                list.Add(item);
                context.Reader.ReadEndDocument();
            }
            context.Reader.ReadEndArray();
            return list;
        }
    }
}
