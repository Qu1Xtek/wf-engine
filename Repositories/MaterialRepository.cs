using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using WorkflowConfigurator.Repositories.Workflow;
using WorkflowConfiguration.Models;
using WorkflowConfigurator.Models.Materials;
using WorkflowConfigurator.Services.Helper.Settings;
using System.Text.RegularExpressions;
using WorkflowConfigurator.Models;

namespace WorkflowConfigurator.Repositories
{
    public class MaterialRepository
    {
        private readonly TempRepo<Material> _tempRepo;

        public MaterialRepository(IConfiguration config)
        {
            _tempRepo = new TempRepo<Material>(config, Configs.MaterialCollectionTable);
            _tempRepo.CreateCompoundIndex(nameof(Material.Name), nameof(Material.MaterialType));
        }

        public async Task<List<Material>> GetAsync() =>
            await _tempRepo.Collection.Find(_ => true).ToListAsync();

        public Material GetOne(string id) =>
            _tempRepo.Collection.Find(x => x.Id == id).First();

        public async Task CreateAsync(Material newWorkflowInstance) =>
            await _tempRepo.Collection.InsertOneAsync(newWorkflowInstance);

        public async Task UpdateAsync(string id, Material updatedWorkflowInstance) =>
            await _tempRepo.Collection.ReplaceOneAsync(x => x.Id == id, updatedWorkflowInstance);

        public async Task UpdateWhereCategoryName(string categoryName, string newCategoryName)
        {
            var filter = Builders<Material>.Filter.Eq(mat => mat.MaterialType, categoryName);
            var update = Builders<Material>.Update.Set(mat => mat.MaterialType, newCategoryName);
            await _tempRepo.Collection.UpdateManyAsync(filter, update);
        }

        private async Task<List<Material>> GetMaterialsByNameAndTypeAsync(string materialName, string materialType)
        {
            var nameFilter = Builders<Material>.Filter.Regex("Name", new Regex(materialName, RegexOptions.IgnoreCase));
            var typeFilter = Builders<Material>.Filter.Regex("MaterialType", new Regex(materialType, RegexOptions.IgnoreCase));
            var compoundFilter = Builders<Material>.Filter.And(nameFilter, typeFilter);

            return await _tempRepo.Collection.Find(compoundFilter).ToListAsync();
        }

        public async Task<GRes<bool>> CreateAsyncIfDoesNotExits(Material material)
        {
            var res = await GetMaterialsByNameAndTypeAsync(material.Name, material.MaterialType);
            if (res != null && res.Count > 0)
            {
                return GRes<bool>.Fault(false, "Material with same name and/or category already exists", 400);
            }
            else
            {
                var createion = CreateAsync(material);
                return GRes<bool>.OK(true, "Material created");
            }
        }

        public async Task RemoveAsync(string id) =>
            await _tempRepo.Collection.DeleteOneAsync(x => x.Id == id);
    }
}
