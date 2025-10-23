using IphoneStoreBE.Common.Models;
using IphoneStoreBE.Services.IServices;
using IphoneStoreBE.VModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IphoneStoreBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        // ✅ Lấy tất cả sản phẩm (có phân trang)
        [HttpGet]
        public async Task<IActionResult> GetAllAsync(int page = 1, int pageSize = 12)
        {
            var result = await _productService.GetAllAsync();
            if (!result.Success || result.Data == null)
                return Ok(ResponseResult<PagedEntity<ProductGetVModel>>.Fail("Không có dữ liệu sản phẩm."));

            var paged = new PagedEntity<ProductGetVModel>(result.Data, page, pageSize);
            return Ok(ResponseResult<PagedEntity<ProductGetVModel>>.Ok(paged, "Lấy danh sách sản phẩm thành công!"));
        }

        // ✅ Lấy chi tiết sản phẩm
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var result = await _productService.GetByIdAsync(id);
            if (!result.Success || result.Data == null)
                return NotFound(ResponseResult.Fail("Không tìm thấy sản phẩm."));

            return Ok(result);
        }

        // ➕ Thêm sản phẩm mới (Admin only)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAsync([FromBody] ProductCreateVModel model)
        {
            var result = await _productService.CreateAsync(model);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // ✏️ Cập nhật sản phẩm (Admin only)
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] ProductUpdateVModel model)
        {
            if (id != model.Id)
                return BadRequest(ResponseResult.Fail("Id không khớp."));

            var result = await _productService.UpdateAsync(model);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // 🗑️ Xóa sản phẩm (Admin only)
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var result = await _productService.DeleteAsync(id);
            
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // ✅ Tìm kiếm sản phẩm theo từ khóa
        [HttpGet("Search")]
        public async Task<IActionResult> SearchAsync([FromQuery] string keyword, int page = 1, int pageSize = 12)
        {
            var all = await _productService.GetAllAsync();
            if (!all.Success || all.Data == null)
                return Ok(ResponseResult<PagedEntity<ProductGetVModel>>.Fail("Không có dữ liệu."));

            var filtered = all.Data
                .Where(p => !string.IsNullOrEmpty(p.ProductName)
                            && p.ProductName.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                            && (p.IsActive ?? true))
                .ToList();

            var paged = new PagedEntity<ProductGetVModel>(filtered, page, pageSize);
            return Ok(ResponseResult<PagedEntity<ProductGetVModel>>.Ok(paged, "Tìm kiếm thành công."));
        }

        // ✅ Lọc sản phẩm theo CategoryId
        [HttpGet("CategoryById")]
        public async Task<IActionResult> GetByCategoryIdAsync([FromQuery] int categoryId, int page = 1, int pageSize = 12)
        {
            var all = await _productService.GetAllAsync();
            if (!all.Success || all.Data == null)
                return Ok(ResponseResult<PagedEntity<ProductGetVModel>>.Fail("Không có dữ liệu."));

            var filtered = all.Data
                .Where(p => p.CategoryId == categoryId && (p.IsActive ?? true))
                .ToList();

            var paged = new PagedEntity<ProductGetVModel>(filtered, page, pageSize);
            return Ok(ResponseResult<PagedEntity<ProductGetVModel>>.Ok(paged, "Lọc theo danh mục thành công."));
        }

        // ✅ Lọc sản phẩm theo tên danh mục
        [HttpGet("Category/{name}")]
        public async Task<IActionResult> GetByCategoryAsync(string name, int page = 1, int pageSize = 12)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return BadRequest(ResponseResult.Fail("Tên danh mục không hợp lệ."));

                var result = await _productService.GetAllAsync();
                if (!result.Success || result.Data == null)
                    return Ok(ResponseResult<PagedEntity<ProductGetVModel>>.Fail("Không có dữ liệu."));

                var filtered = result.Data
                    .Where(p => !string.IsNullOrEmpty(p.CategoryName) 
                            && p.CategoryName.Equals(name, StringComparison.OrdinalIgnoreCase) 
                            && (p.IsActive ?? true))
                    .ToList();

                if (filtered.Count == 0)
                    return Ok(ResponseResult<PagedEntity<ProductGetVModel>>.Fail($"Không tìm thấy sản phẩm trong danh mục '{name}'."));

                var paged = new PagedEntity<ProductGetVModel>(filtered, page, pageSize);
                return Ok(ResponseResult<PagedEntity<ProductGetVModel>>.Ok(paged, $"Lọc theo danh mục '{name}' thành công."));
            }
            catch (Exception ex)
            {
                return Ok(ResponseResult<PagedEntity<ProductGetVModel>>.Fail($"Lỗi khi lọc sản phẩm theo danh mục: {ex.Message}"));
            }
        }
    }
}
