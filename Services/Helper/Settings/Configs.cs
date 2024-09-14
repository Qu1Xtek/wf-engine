using WorkflowConfigurator.Services.Helper.Settings;

namespace WorkflowConfigurator.Services.Helper.Settings
{
    public static class Configs
    {
        public const string DBConnection = "DBConnection";
        public const string MongoDBName = "MongoDBName";

        public const string Collections = "CollectionNames";

        public const string WFInstanceTable = "WorkflowInstanceTable";
        public const string WFDefinitionTable = "WorkflowDefinitionTable";
        public const string WFDefinitionBackupTable = "WorkflowDefinitionBackupTable";
        public const string UsersCollectionTable = "UsersCollectionTable";
        public const string UserSessionsCollectionTable = "UserSessionsCollectionTable";
        public const string ActivityTimerCollectionTable = "ActivityTimerCollectionTable";
        public const string ArchivedActivityTimerCollectionTable = "ArchivedActivityTimerCollectionTable";
        public const string MaterialCollectionTable = "MaterialCollectionTable";
        public const string WFConfiguration = "Configurations";
        public const string Categories = "CatrgoriesCollectionTable";

        public static ExternalEndpoints ExternalEndpoints = new ExternalEndpoints();
        public static LocalCacheConstants LocalCache = new LocalCacheConstants();

        public const string UserMGMTSection = "UserManagementURLS";
        public const string IMSSection = "IMSURLS";
        public const string KCBLASTDEVID = "KCBLASTDEVID";
        public const string BLASTDEVKCSECRET = "BLASTDEVKCSECRET";
    }
}
