using Microsoft.AspNetCore.Mvc;
using WorkflowConfigurator.Models;
using WorkflowConfigurator.Models.Activity;
using WorkflowConfigurator.Models.Materials;
using WorkflowConfigurator.Repositories;
using WorkflowConfigurator.Services.Materials;
using WorkflowConfigurator.Services.Service;

namespace WorkflowConfigurator.Controllers.Materials
{
    [ApiController]
    [Route("Category")]
    public class CategoryController : ControllerBase
    {

        private readonly CategoryService _matCategoryService;


        public CategoryController(
            WorkflowDefinitionRepository workflowDefinitionService,
            CategoryService matCatService)
        {
            _matCategoryService = matCatService;
        }

        [HttpGet]
        public async Task<List<Category>> GetCateogires()
        {
            return await _matCategoryService.GetAllAsync();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            var res = await _matCategoryService.CreateAsyncIfDoesNotExits(category);

            if (res.IsSuccesful)
            {
                return Ok(res);
            }
            else
            {
                return BadRequest(res);
            }
        }
        [HttpPut]
        public async Task UpdateCategory(Category category)
        {
            await _matCategoryService.UpdateCategoryName(category);
        }


    }
}