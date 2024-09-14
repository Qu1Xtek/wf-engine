using WorkflowConfigurator.Models;
using WorkflowConfigurator.Models.Materials;
using WorkflowConfigurator.Repositories;

namespace WorkflowConfigurator.Services.Materials
{
    public class CategoryService
    {
        private readonly MatCategoryRepository _categoryRepository;
        private readonly MaterialService _materialService;

        public CategoryService(MatCategoryRepository categoryRepository, MaterialService materialService)
        {
            _categoryRepository = categoryRepository;
            _materialService = materialService;
        }

        public async Task<List<Category>> GetAllAsync() =>
            await _categoryRepository.GetAsync();

        public async Task CreateAsync(Category newCategory) =>
            await _categoryRepository.CreateAsync(newCategory);


        public async Task UpdateAsync(Category category)
        {
            await _categoryRepository.UpdateAsync(category.Id, category);


        }

        public async Task UpdateCategoryName(Category category)
        {
            var oldName = await _categoryRepository.GetByIdAsync(category.Id);

            await UpdateAsync(category);
            await _materialService.UpdateWhereCategoryName(oldName.Name, category.Name);
            
        }

        public async Task<GRes<bool>> CreateAsyncIfDoesNotExits(Category category)
        {
            return await _categoryRepository.CreateAsyncIfDoesNotExits(category);
        }
    }
}
