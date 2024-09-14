using MongoDB.Driver;
using WorkflowConfigurator.Services.Helper.Settings;

namespace WorkflowConfigurator.Services.Helper
{
    public class MongoDBHelper
    {
        private static MongoClientSettings ClientSettings;
        private static string DevConnection;

        private static bool IsDebug;

        public static void InitialSetup(string envName , IConfiguration config)
        {

            IsDebug = envName == "Development";


            if (IsDebug)
            {
                Console.WriteLine("Development mode - initializing dev DB");
                DevConnection = config.GetConnectionString(Configs.DBConnection);
            }
            else
            {
                Console.WriteLine("PRODUCTION mode - PRODUCTION DB SETUP");
                var connectionString = config.GetConnectionString(Configs.DBConnection);
                ClientSettings = MongoClientSettings.FromConnectionString(connectionString);

                ClientSettings.Credential = new MongoCredential(
                    "MONGODB-AWS",
                    new MongoExternalAwsIdentity(),
                    new ExternalEvidence());
            }
        }

        public static MongoClient GetClient()
        {
            if(IsDebug)
            {
                return new MongoClient(DevConnection);
            }
            else
            {
                return new MongoClient(ClientSettings);
            }
        }
    }
}
