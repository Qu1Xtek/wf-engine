using MongoDB.Driver;
using WorkflowConfigurator.Services.Helper.Settings;

namespace WorkflowConfigurator.Repositories.Workflow
{
    public class TempRepo<T> : BaseMongoRepository<T>
    {
        private readonly string _colName;

        public TempRepo(IConfiguration config, string collectionName) : base(config, collectionName) 
        {
            _colName = collectionName;
        }

        public IMongoCollection<T> Collection => _collection;
        protected override string GetCollectionName(IConfiguration config)
        {
            if (_tempCol == null)
            {
                return config
                .GetSection(Configs.Collections)
                .GetValue<string>(_colName);
            }
            else
            {
                return config
                .GetSection(Configs.Collections)
                .GetValue<string>(_tempCol);
            }
        }

        public string CreateUniqueIndex(string fieldName)
        {
            var options = new CreateIndexOptions() { Unique = true };
            var field = new StringFieldDefinition<T>(fieldName);
            var indexDefinition = new IndexKeysDefinitionBuilder<T>().Ascending(field);
            var result = _collection.Indexes.CreateOne(indexDefinition, options);

            return result;
        }
    }
}
