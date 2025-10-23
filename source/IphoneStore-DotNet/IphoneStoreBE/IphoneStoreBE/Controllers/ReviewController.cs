using IphoneStoreBE.Context;
using IphoneStoreBE.Services.IServices;
using IphoneStoreBE.VModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IphoneStoreBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : BaseController
    {
        private readonly IReviewService _reviewService;
        private readonly IphoneStoreContext _context;

        public ReviewController(IReviewService reviewService, IphoneStoreContext context)
        {
            _reviewService = reviewService;
            _context = context;
        }

        // [1.] Lấy tất cả Review (filter theo productId query param, public)
        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] int? productId = null)
        {
            var result = await _reviewService.GetAllAsync(productId);
            return HandleResponse(result);
        }

        // [2.] Lấy Review theo ID (public)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _reviewService.GetByIdAsync(id);
            return HandleResponse(result);
        }

        // [3.] Tạo mới Review (auth required)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateReview([FromBody] ReviewCreateVModel model)
        {
            var result = await _reviewService.CreateAsync(model, HttpContext);
            return HandleResponse(result);
        }

        // [4.] Xóa Review (auth required)
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var result = await _reviewService.DeleteAsync(id, HttpContext);
            return HandleResponse(result);
        }

        // [5.] Cập nhật Review (auth required)
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateReview([FromBody] ReviewUpdateVModel model)
        {
            var result = await _reviewService.UpdateAsync(model, HttpContext);
            return HandleResponse(result);
        }
    }
}