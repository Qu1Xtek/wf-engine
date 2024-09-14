using MongoDB.Driver;
using System.Text.RegularExpressions;
using WorkflowConfigurator.Models;
using WorkflowConfigurator.Models.Materials;
using WorkflowConfigurator.Repositories.Workflow;
using WorkflowConfigurator.Services.Helper.Settings;

namespace WorkflowConfigurator.Repositories
{
    public class MatCategoryRepository
    {
        private readonly TempRepo<Category> _tempRepo;

        public MatCategoryRepository(IConfiguration config)
        {
            _tempRepo = new TempRepo<Category>(config, Configs.Categories);
            _tempRepo.CreateUniqueIndex(nameof(Category.Name));
        }

        public async Task<List<Category>> GetAsync() =>
            await _tempRepo.Collection.Find(_ => true).ToListAsync();

        public Category GetOne(string id) =>
            _tempRepo.Collection.Find(x => x.Id == id).First();
        public async Task<Category> GetByIdAsync(string id) =>
            await _tempRepo.Collection.Find(x => x.Id == id).FirstAsync();
        public async Task<Category> GetByNameAsync(string name) =>
            await _tempRepo.Collection.Find(x => x.Name == name).FirstAsync();

        public async Task CreateAsync(Category newCategory) =>
            await _tempRepo.Collection.InsertOneAsync(newCategory);
        private async Task<List<Category>> GetCategoryByNameAsync(string categoryName)
        {
            var nameFilter = Builders<Category>.Filter.Regex("Name", new Regex(categoryName, RegexOptions.IgnoreCase));
            var compoundFilter = Builders<Category>.Filter.And(nameFilter);

            return await _tempRepo.Collection.Find(compoundFilter).ToListAsync();
        }
        public async Task<GRes<bool>> CreateAsyncIfDoesNotExits(Category category)
        {
            var res = await GetCategoryByNameAsync(category.Name);
            if (res != null && res.Count > 0)
            {
                return GRes<bool>.Fault(false, "Category with same name already exists", 400);
            }
            else
            {
                await CreateAsync(category);
                return GRes<bool>.OK(true, "Category created");
            }
        }
        public async Task UpdateAsync(string id, Category updatednewCategory) =>
            await _tempRepo.Collection.ReplaceOneAsync(x => x.Id == id, updatednewCategory);

        public async Task RemoveAsync(string id) =>
            await _tempRepo.Collection.DeleteOneAsync(x => x.Id == id);
    }
}
