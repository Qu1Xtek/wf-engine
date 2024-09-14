using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WorkflowConfigurator.Services.Helper;
using WorkflowConfigurator.Services.Helper.Settings;

namespace WorkflowConfigurator.Repositories
{
    public abstract class BaseMongoRepository<T>
    {
        /// <summary>
        /// This is bypass value and it will be only used in TempRepo, 
        /// this property should be removed, not part of the desired architecture
        /// </summary>
        protected string _tempCol;

        protected abstract string GetCollectionName(IConfiguration config);
        
        protected IMongoCollection<T> _collection;
        
        public BaseMongoRepository(IConfiguration config, string tempCol = null)
        {
            _tempCol = tempCol;
            MongoDbConventionRegistration.RegisterConventions();
            var cli = MongoDBHelper.GetClient();
            var db = cli.GetDatabase(config.GetValue<string>(Configs.MongoDBName));
            var name = GetCollectionName(config);
            _collection = db.GetCollection<T>(name);
        }

        public string CreateUniqueIndex(string fieldName)
        {
            var options = new CreateIndexOptions() { Unique = true };
            var field = new StringFieldDefinition<T>(fieldName);
            var indexDefinition = new IndexKeysDefinitionBuilder<T>().Ascending(field);
            var result = _collection.Indexes.CreateOne(indexDefinition, options);

            return result;
        }

        public string CreateCompoundIndex(string fieldName, string fieldName2)
        {
            var options = new CreateIndexOptions() { Unique = true };
            var field = new StringFieldDefinition<T>(fieldName);
            var field2 = new StringFieldDefinition<T>(fieldName2);
            var indexDefinition = new IndexKeysDefinitionBuilder<T>()
                .Ascending(field)
                .Ascending(field2);

            var result = _collection.Indexes.CreateOne(indexDefinition, options);

            return result;
        }
    }
}
