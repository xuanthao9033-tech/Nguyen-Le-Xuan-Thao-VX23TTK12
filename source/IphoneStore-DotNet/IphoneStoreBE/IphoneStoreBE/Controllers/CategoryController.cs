using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using IphoneStoreBE.Context;
using IphoneStoreBE.Services.IServices;
using IphoneStoreBE.VModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IphoneStoreBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : BaseController
    {
        private readonly ICategoryService _categoryService;
        private readonly IphoneStoreContext _context;

        // Constructor injection for services
        public CategoryController(ICategoryService categoryService, IphoneStoreContext context)
        {
            _categoryService = categoryService;
            _context = context;
        }

        // [1.] Lấy tất cả Danh mục  
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            // B1: Gọi service để lấy danh sách tất cả danh mục
            var result = await _categoryService.GetAllAsync();

            // B2: Trả về kết quả
            return HandleResponse(result);
        }

        // [2.] Lấy Danh mục theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            // B1: Gọi service để lấy danh mục theo ID
            var result = await _categoryService.GetByIdAsync(id);

            // B2: Trả về kết quả
            return HandleResponse(result);
        }

        // [3.] Tạo mới Danh mục
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateVModel categoryVModel)
        {
            // B2: Gọi service để tạo mới danh mục
            var result = await _categoryService.CreateAsync(categoryVModel);

            // B2: Trả về kết quả
            return HandleResponse(result);
        }

        // [4.] Xóa Danh mục
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            // B1: Gọi service để xóa danh mục
            var result = await _categoryService.DeleteAsync(id);

            // B2: Trả về kết quả
            return HandleResponse(result);
        }

        // [5.] Cập nhật Danh mục
        [HttpPut]
        public async Task<IActionResult> UpdateCategory([FromBody] CategoryUpdateVModel categoryVModel)
        {
            // B1: Gọi service để cập nhật danh mục
            var result = await _categoryService.UpdateAsync(categoryVModel);

            // B2: Trả về kết quả
            return HandleResponse(result);
        }
    }
}
