using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace WorkflowConfigurator.Services.Helper
{
    public class CustomObjectIdGenerator : IIdGenerator
    {
        public object GenerateId(object container, object document)
        {
            return ObjectId.GenerateNewId().ToString();
        }

        public bool IsEmpty(object id)
        {
            return id == null || string.IsNullOrEmpty(id.ToString());
        }

        public void SetDocumentId(object document, object id)
        {
            // Do nothing as the document already has the correct Id
        }
    }
}
