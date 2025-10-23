using IphoneStoreBE.Context;
using IphoneStoreBE.Services.IServices;
using IphoneStoreBE.VModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IphoneStoreBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductImageController : BaseController
    {
        private readonly IProductImageService _productImageService;
        private readonly IphoneStoreContext _context;

        public ProductImageController(IProductImageService productImageService, IphoneStoreContext context)
        {
            _productImageService = productImageService;
            _context = context;
        }

        // [1.] Lấy tất cả ảnh (filter theo productId query param)
        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] int? productId = null)
        {
            var result = await _productImageService.GetAllAsync(productId);
            return HandleResponse(result);
        }

        // [2.] Lấy ảnh theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _productImageService.GetByIdAsync(id);
            return HandleResponse(result);
        }

        // [3.] Tạo mới ảnh (multipart/form-data)
        [HttpPost]
        public async Task<IActionResult> CreateProductImage([FromForm] ProductImageCreateVModel model, IFormFile imageFile)
        {
            var result = await _productImageService.CreateAsync(model, imageFile);
            return HandleResponse(result);
        }

        // [4.] Xóa ảnh
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductImage(int id)
        {
            var result = await _productImageService.DeleteAsync(id);
            return HandleResponse(result);
        }

        // [5.] Cập nhật ảnh
        [HttpPut]
        public async Task<IActionResult> UpdateProductImage([FromBody] ProductImageUpdateVModel model)
        {
            var result = await _productImageService.UpdateAsync(model);
            return HandleResponse(result);
        }
    }
}