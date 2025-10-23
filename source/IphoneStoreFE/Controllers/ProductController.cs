using IphoneStoreBE.Common.Models;
using IphoneStoreBE.VModels;
using IphoneStoreFE.Models;
using IphoneStoreFE.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace IphoneStoreFE.Controllers
{
    public class ProductController : Controller
    {
        private readonly ProductService _productService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Constructor to inject ProductService and IHttpContextAccessor
        public ProductController(ProductService productService, IHttpContextAccessor httpContextAccessor)
        {
            _productService = productService;
            _httpContextAccessor = httpContextAccessor;
        }

        // Trang chủ hiển thị danh sách sản phẩm
        public async Task<IActionResult> Index(int page = 1)
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext?.Session.GetInt32("UserId");
                ViewBag.UserId = userId ?? 0;

                var pagedProducts = await _productService.GetAllProductsAsync(page);
                return View(pagedProducts);
            }
            catch (Exception)
            {
                // Handle exception if needed
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        // Lọc sản phẩm theo danh mục
        public async Task<IActionResult> Category(string name, int page = 1)
        {
            try
            {
                var userId = HttpContext.Session.GetInt32("UserId");
                ViewBag.UserId = userId ?? 0;
                ViewBag.CategoryName = name;

                var products = await _productService.GetProductsByCategoryAsync(name, page);
                return View(products);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ProductController] Error getting products by category: {ex.Message}");
                return View("Error");
            }
        }

        // Tìm kiếm sản phẩm
        public async Task<IActionResult> Search(string keyword, int page = 1)
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext?.Session.GetInt32("UserId");
                ViewBag.UserId = userId ?? 0;
                ViewBag.SearchKeyword = keyword;

                var products = await _productService.SearchProductsAsync(keyword, page);
                return View(products);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ProductController] Lỗi khi tìm kiếm sản phẩm: {ex.Message}");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }

        // Chi tiết sản phẩm
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext?.Session.GetInt32("UserId");
                ViewBag.UserId = userId ?? 0;

                var product = await _productService.GetProductByIdAsync(id);

                if (product == null)
                {
                    return NotFound();
                }

                return View(product);  // Trả về view chi tiết sản phẩm
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ProductController] Lỗi khi tải chi tiết sản phẩm: {ex.Message}");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
    }
}
