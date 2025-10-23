using Microsoft.AspNetCore.Mvc;
using IphoneStoreBE.Common.Models;
using IphoneStoreBE.VModels;
using IphoneStoreFE.Services;

namespace IphoneStoreFE.Controllers
{
    public class HomeController : Controller
    {
        private readonly ProductService _productService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(ProductService productService, IHttpContextAccessor httpContextAccessor)
        {
            _productService = productService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            // Get userId from session
            var userId = _httpContextAccessor.HttpContext?.Session.GetInt32("UserId");
            ViewBag.UserId = userId ?? 0;

            // Get products with paging
            var result = await _productService.GetAllProductsAsync(page);
            
            if (result.IsSuccess && result.Data != null)
            {
                return View(result.Data);
            }
            
            TempData["ErrorMessage"] = result.Message ?? "Không thể tải danh sách sản phẩm";
            return View(new PagedEntity<ProductGetVModel>(new List<ProductGetVModel>(), page, 12));
        }
    }
}
