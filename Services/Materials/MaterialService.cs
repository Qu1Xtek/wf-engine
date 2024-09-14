using WorkflowConfigurator.Models.Materials;
using WorkflowConfigurator.Services.Helper.Settings;
using WorkflowConfigurator.Models;
using WorkflowConfigurator.Repositories;
using WorkflowConfigurator.Models.DTO;
using Newtonsoft.Json;
using Microsoft.AspNetCore.WebUtilities;

namespace WorkflowConfigurator.Services.Materials
{
    public class MaterialService
    {
        private readonly MaterialRepository _materialRepository;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly string _getProductsAddress;
        private readonly string _checkProductAddress;

        public MaterialService(MaterialRepository materialRepository, HttpClient client, IConfiguration config)
        {
            _materialRepository = materialRepository;
            _httpClient = client;
            _config = config;
            _getProductsAddress = _config
                .GetSection(Configs.IMSSection)
                .GetValue<string>(Configs.ExternalEndpoints.GetCategories);

            _checkProductAddress = _config
                .GetSection(Configs.IMSSection)
                .GetValue<string>(Configs.ExternalEndpoints.CheckProduct);

            var baseAddr = _config.GetSection(Configs.IMSSection).GetValue<string>(
                Configs.ExternalEndpoints.BaseURL);

            _httpClient.BaseAddress = new Uri(baseAddr);
        }

        public async Task<List<ProductDTO>> GetCategories(int code)
        {
            Dictionary<string, string?> prms = new() 
            {
                { "code", code.ToString() } 
            };

            var queryUri = QueryHelpers.AddQueryString(
                _getProductsAddress,
                prms);

            var request = new HttpRequestMessage(
                HttpMethod.Get,
                queryUri);

            
            var result = await _httpClient.SendAsync(request);

            var response = JsonConvert.DeserializeObject<GRes<List<ProductDTO>>>(
                await result.Content.ReadAsStringAsync());

            return response.Data;

        }
         
        public Material GetById(string id) =>
            _materialRepository.GetOne(id);
        public async Task CreateAsync(Material material) =>
            await _materialRepository.CreateAsync(material);


        public async Task<GRes<bool>> CreateAsyncIfDoesNotExits(Material material)
        {
            return await _materialRepository.CreateAsyncIfDoesNotExits(material);
        }

        public async Task UpdateAsync(Material updatedMaterial) =>
            await _materialRepository.UpdateAsync(updatedMaterial.Id, updatedMaterial);

        public async Task UpdateWhereCategoryName(string categoryName, string newCategoryName) {
            await _materialRepository.UpdateWhereCategoryName(categoryName, newCategoryName);
        }

        public async Task<bool> CheckForInvalidScan(string scannedValue, string scanType)
        {
            //var uri = new Uri();

            var request = new HttpRequestMessage(
                HttpMethod.Post,
                _checkProductAddress);

            var serializedObj = JsonConvert.SerializeObject(new ProductDTO
            {
                Id = scannedValue,
                ProductName = scanType
            });

            var content = new StringContent(serializedObj, null, "application/json");

            request.Content = content;
            
            var result = await _httpClient.SendAsync(request);

            var response = JsonConvert.DeserializeObject<GRes<bool>>(
                await result.Content.ReadAsStringAsync());

            return response.Data;
        }
    }
}
