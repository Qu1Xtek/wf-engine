using MongoDB.Bson.Serialization.Conventions;

namespace WorkflowConfigurator.Services.Helper
{
    public class MongoDbConventionRegistration
    {
        public static void RegisterConventions()
        {
            ConventionRegistry.Register(
                "CustomObjectIdConvention",
                new ConventionPack { new ObjectIdToStringConvention() },
                t => true
            );
        }
    }

}
