using WorkflowConfigurator.Models.Authorization;
using WorkflowConfigurator.Services.Helper.Settings;

namespace WorkflowConfigurator.Repositories.Workflow
{
    public class UserRepository : BaseMongoRepository<User>
    {
        public UserRepository(IConfiguration config) : base(config) { }

        protected override string GetCollectionName(IConfiguration config)
        {
            return config
                .GetSection(Configs.Collections)
                .GetValue<string>(Configs.UsersCollectionTable);
        }

        public async Task<bool> Create(User user)
        {
            try
            {
                await _collection.InsertOneAsync(user);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }
}
