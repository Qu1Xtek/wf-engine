using MongoDB.Driver;
using WorkflowConfigurator.Models.Workflow;
using WorkflowConfigurator.Services.Helper.Settings;

namespace WorkflowConfigurator.Repositories.Workflow
{
    public class WFInstanceRepo : BaseMongoRepository<WorkflowInstance>
    {
        public WFInstanceRepo(IConfiguration config) : base(config) { }

        protected override string GetCollectionName(IConfiguration config)
        {
            return config
                .GetSection(Configs.Collections)
                .GetValue<string>(Configs.WFInstanceTable);
        }

        public async Task<List<WorkflowInstance>> GetFromTo(DateTime from, DateTime to, string companyId)
        {
            var cursor = await _collection.FindAsync(x => x.CreatedOn > from
                    && x.CreatedOn < to
                    && x.CompanyId.ToLowerInvariant() == companyId.ToLowerInvariant());

            return await cursor.ToListAsync();
        }

        public async Task<List<WorkflowInstance>> GetRecent(string companyId)
        {
            var cursor = _collection
                .Find(x=> x.CompanyId.ToLowerInvariant() == companyId.ToLowerInvariant())
                .Sort(Builders<WorkflowInstance>.Sort.Descending("_id"))
                .Limit(20);

            return await cursor.ToListAsync();
        }
    }
}
