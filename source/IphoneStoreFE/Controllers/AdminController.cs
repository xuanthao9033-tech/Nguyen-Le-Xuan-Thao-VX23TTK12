using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IphoneStoreFE.Services;
using IphoneStoreFE.Models;
using IphoneStoreBE.VModels;

namespace IphoneStoreFE.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly IWebHostEnvironment _environment;

        public AdminController(
            ProductService productService, 
            IOrderService orderService,
            IHttpContextAccessor httpContextAccessor,
            IHttpClientFactory httpClientFactory,
            IWebHostEnvironment environment)
        {
            _productService = productService;
            _orderService = orderService;
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClientFactory.CreateClient("IphoneStoreAPI");
            _environment = environment;
        }

        // 🏠 Dashboard tổng quan với thống kê
        public async Task<IActionResult> Index()
        {
            try
            {
                // Lấy tất cả đơn hàng để tính toán thống kê
                var allOrdersResult = await _orderService.GetAllOrdersAsync(1, 1000); // Get all orders
                
                if (allOrdersResult.Success && allOrdersResult.Data != null)
                {
                    var orders = allOrdersResult.Data.Items;
                    
                    // Thống kê đơn hàng
                    ViewBag.TotalOrders = orders.Count;
                    ViewBag.DeliveredOrders = orders.Count(o => o.OrderStatus == "Đã giao thành công");
                    ViewBag.PendingOrders = orders.Count(o => o.OrderStatus == "Chờ xác nhận");
                    ViewBag.TotalRevenue = orders.Where(o => o.OrderStatus == "Đã giao thành công").Sum(o => o.Total);
                    
                    // Sản phẩm được đặt nhiều nhất (cần gọi API riêng hoặc tính từ OrderDetails)
                    // Tạm thời để null, sẽ implement sau
                    ViewBag.TopProducts = new List<dynamic>();
                }
                else
                {
                    ViewBag.TotalOrders = 0;
                    ViewBag.DeliveredOrders = 0;
                    ViewBag.PendingOrders = 0;
                    ViewBag.TotalRevenue = 0;
                    ViewBag.TopProducts = new List<dynamic>();
                }
                
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi tải dashboard: {ex.Message}";
                return View();
            }
        }

        // 📦 QUẢN LÝ SẢN PHẨM
        public async Task<IActionResult> Products(int page = 1, int pageSize = 10)
        {
            try
            {
                var result = await _productService.GetAllProductsAsync(page, pageSize);
                
                if (result.IsSuccess && result.Data != null)
                {
                    return View(result.Data);
                }
                
                TempData["ErrorMessage"] = result.Message ?? "Không thể tải danh sách sản phẩm";
                return View(new IphoneStoreFE.Models.PagedEntity<ProductGetVModel>(new List<ProductGetVModel>(), page, pageSize));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                return View(new IphoneStoreFE.Models.PagedEntity<ProductGetVModel>(new List<ProductGetVModel>(), page, pageSize));
            }
        }

        // ➕ Thêm sản phẩm mới
        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        {
            try
            {
                await LoadCategoriesAsync();
                
                // Return empty model for form binding
                var model = new ProductCreateVModel
                {
                    IsActive = true // Default value
                };
                
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi tải form: {ex.Message}";
                await LoadCategoriesAsync();
                return View(new ProductCreateVModel { IsActive = true });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(ProductCreateVModel model, IFormFile? imageFile, string? CategoryName)
        {
            try
            {
                // ✅ Validate basic model
                if (!ModelState.IsValid)
                {
                    await LoadCategoriesAsync();
                    return View(model);
                }

                // ✅ Validate SKU
                if (string.IsNullOrWhiteSpace(model.Sku))
                {
                    ModelState.AddModelError(nameof(model.Sku), "SKU không được để trống");
                    await LoadCategoriesAsync();
                    return View(model);
                }

                // ✅ Xử lý category: tìm hoặc tạo mới
                var categoryResult = await ProcesscategoryAsync(CategoryName);
                if (!categoryResult.IsSuccess)
                {
                    TempData["ErrorMessage"] = categoryResult.Message;
                    await LoadCategoriesAsync();
                    return View(model);
                }
                model.CategoryId = categoryResult.Data;

                // ✅ Ensure IsActive has a value
                if (!model.IsActive.HasValue)
                {
                    model.IsActive = true;
                }

                // ✅ Upload ảnh nếu có
                var imageResult = await ProcessImageUploadAsync(imageFile, model.Sku);
                if (!imageResult.IsSuccess)
                {
                    TempData["ErrorMessage"] = imageResult.Message;
                    await LoadCategoriesAsync();
                    return View(model);
                }
                
                if (!string.IsNullOrEmpty(imageResult.Data))
                {
                    model.ImageUrl = imageResult.Data;
                }

                // ✅ Tạo sản phẩm thông qua service
                var createResult = await _productService.CreateProductAsync(model);
                
                if (createResult.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Thêm sản phẩm thành công!";
                    return RedirectToAction("Products");
                }
                else
                {
                    TempData["ErrorMessage"] = createResult.Message;
                    await LoadCategoriesAsync();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                await LoadCategoriesAsync();
                return View(model);
            }
        }

        // ✏️ Cập nhật sản phẩm
        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            try
            {
                // Validate input
                if (id <= 0)
                {
                    TempData["ErrorMessage"] = "ID sản phẩm không hợp lệ";
                    return RedirectToAction("Products");
                }

                var result = await _productService.GetProductByIdAsync(id);
                if (!result.IsSuccess || result.Data == null)
                {
                    TempData["ErrorMessage"] = result.Message ?? "Không tìm thấy sản phẩm";
                    return RedirectToAction("Products");
                }

                var product = result.Data;
                await LoadCategoriesAsync();

                // Convert ProductGetVModel to ProductUpdateVModel
                var updateModel = new ProductUpdateVModel
                {
                    Id = product.Id,
                    ProductName = product.ProductName,
                    Sku = product.Sku,
                    Price = product.Price,
                    SpecificationDescription = product.SpecificationDescription,
                    Warranty = product.Warranty,
                    ProductType = product.ProductType,
                    Color = product.Color,
                    Capacity = product.Capacity,
                    ImageUrl = product.ImageUrl,
                    IsActive = product.IsActive,
                    CategoryId = product.CategoryId ?? 0
                };

                return View(updateModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi tải sản phẩm: {ex.Message}";
                return RedirectToAction("Products");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(ProductUpdateVModel model, IFormFile? imageFile)
        {
            try
            {
                // ✅ Validate basic model
                if (!ModelState.IsValid)
                {
                    await LoadCategoriesAsync();
                    return View(model);
                }

                // ✅ Validate SKU
                if (string.IsNullOrWhiteSpace(model.Sku))
                {
                    ModelState.AddModelError(nameof(model.Sku), "SKU không được để trống");
                    await LoadCategoriesAsync();
                    return View(model);
                }

                // ✅ Validate IsActive has a value
                if (!model.IsActive.HasValue)
                {
                    model.IsActive = true;
                }

                // ✅ Upload ảnh mới nếu có
                if (imageFile != null)
                {
                    var imageResult = await ProcessImageUploadAsync(imageFile, model.Sku, model.ImageUrl);
                    if (!imageResult.IsSuccess)
                    {
                        TempData["ErrorMessage"] = imageResult.Message;
                        await LoadCategoriesAsync();
                        return View(model);
                    }
                    
                    if (!string.IsNullOrEmpty(imageResult.Data))
                    {
                        model.ImageUrl = imageResult.Data;
                    }
                }

                // ✅ Cập nhật sản phẩm thông qua service
                var updateResult = await _productService.UpdateProductAsync(model);
                
                if (updateResult.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                    return RedirectToAction("Products");
                }
                else
                {
                    TempData["ErrorMessage"] = updateResult.Message;
                    await LoadCategoriesAsync();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                await LoadCategoriesAsync();
                return View(model);
            }
        }

        // 🗑️ Xóa sản phẩm
        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                // Validate input
                if (id <= 0)
                {
                    TempData["ErrorMessage"] = "ID sản phẩm không hợp lệ";
                    return RedirectToAction("Products");
                }

                var result = await _productService.DeleteProductAsync(id);
                
                if (result.IsSuccess)
                {
                    TempData["SuccessMessage"] = "Xóa sản phẩm thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = result.Message ?? "Không thể xóa sản phẩm";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
            }

            return RedirectToAction("Products");
        }

        // 📋 QUẢN LÝ ĐƠN HÀNG
        public async Task<IActionResult> Orders(int page = 1)
        {
            try
            {
                // Debug: Check if service is available
                if (_orderService == null)
                {
                    TempData["ErrorMessage"] = "OrderService is NULL";
                    return View(new PagedEntity<OrderGetVModel>(new List<OrderGetVModel>(), 1, 20));
                }

                // Call service
                var result = await _orderService.GetAllOrdersAsync(page);
                
                // Debug logging
                System.Diagnostics.Debug.WriteLine($"Orders - Success: {result.Success}");
                System.Diagnostics.Debug.WriteLine($"Orders - Message: {result.Message}");
                System.Diagnostics.Debug.WriteLine($"Orders - Data is null: {result.Data == null}");
                
                if (result.Data != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Orders - Items count: {result.Data.Items?.Count ?? 0}");
                    System.Diagnostics.Debug.WriteLine($"Orders - Total items: {result.Data.TotalItems}");
                }
                
                if (result.Success && result.Data != null)
                {
                    return View(result.Data); // ✅ Pass only PagedEntity, not ResponseResult
                }
                
                TempData["ErrorMessage"] = result.Message ?? "Không thể tải danh sách đơn hàng. Vui lòng thử lại.";
                return View(new PagedEntity<OrderGetVModel>(new List<OrderGetVModel>(), 1, 20));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Orders - Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Orders - Stack Trace: {ex.StackTrace}");
                
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                return View(new PagedEntity<OrderGetVModel>(new List<OrderGetVModel>(), 1, 20));
            }
        }

        // 📄 Chi tiết đơn hàng
        public async Task<IActionResult> OrderDetail(int id)
        {
            try
            {
                var result = await _orderService.GetOrderByIdAsync(id);
                
                if (result.Success && result.Data != null)
                {
                    return View(result.Data);
                }
                
                TempData["ErrorMessage"] = result.Message ?? "Không thể tải chi tiết đơn hàng";
                return RedirectToAction("Orders");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("Orders");
            }
        }

        // ✅ Cập nhật trạng thái đơn hàng
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int id, string status)
        {
            try
            {
                var result = await _orderService.UpdateOrderStatusAsync(id, status);
                
                if (result.Success)
                {
                    return Json(new { success = true, message = result.Message });
                }

                return Json(new { success = false, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Load categories for dropdown
        /// </summary>
        private async Task LoadCategoriesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("category");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<IphoneStoreBE.Common.Models.ResponseResult<List<CategoryGetVModel>>>();
                    ViewBag.Categories = result?.Data ?? new List<CategoryGetVModel>();
                }
                else
                {
                    ViewBag.Categories = new List<CategoryGetVModel>();
                }
            }
            catch
            {
                ViewBag.Categories = new List<CategoryGetVModel>();
            }
        }

        /// <summary>
        /// Process category - find existing or create new
        /// </summary>
        private async Task<ServiceResult<int>> ProcesscategoryAsync(string? CategoryName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CategoryName))
                {
                    return ServiceResult<int>.Failure("Vui lòng nhập tên danh mục");
                }

                // Lấy danh sách categories hiện có
                var response = await _httpClient.GetAsync("category");
                if (!response.IsSuccessStatusCode)
                {
                    return ServiceResult<int>.Failure("Không thể kiểm tra danh mục");
                }

                var result = await response.Content.ReadFromJsonAsync<IphoneStoreBE.Common.Models.ResponseResult<List<CategoryGetVModel>>>();
                var categories = result?.Data ?? new List<CategoryGetVModel>();
                
                // Tìm category theo tên (không phân biệt hoa thường)
                var existingcategory = categories.FirstOrDefault(c => 
                    c.CategoryName.Equals(CategoryName.Trim(), StringComparison.OrdinalIgnoreCase));
                
                if (existingcategory != null)
                {
                    return ServiceResult<int>.Success(existingcategory.Id);
                }

                // Tạo category mới
                var newcategory = new CategoryCreateVModel
                {
                    CategoryName = CategoryName.Trim(),
                    IsActive = true
                };
                
                var createResponse = await _httpClient.PostAsJsonAsync("category", newcategory);
                if (!createResponse.IsSuccessStatusCode)
                {
                    return ServiceResult<int>.Failure("Không thể tạo danh mục mới");
                }

                var createResult = await createResponse.Content.ReadFromJsonAsync<IphoneStoreBE.Common.Models.ResponseResult<CategoryGetVModel>>();
                if (createResult?.Data != null)
                {
                    return ServiceResult<int>.Success(createResult.Data.Id);
                }

                return ServiceResult<int>.Failure("Không thể tạo danh mục mới");
            }
            catch (Exception ex)
            {
                return ServiceResult<int>.Failure($"Lỗi xử lý danh mục: {ex.Message}");
            }
        }

        /// <summary>
        /// Process image upload
        /// </summary>
        private async Task<ServiceResult<string>> ProcessImageUploadAsync(IFormFile? imageFile, string sku, string? oldImageUrl = null)
        {
            try
            {
                if (imageFile == null)
                {
                    return ServiceResult<string>.Success(string.Empty);
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(extension))
                {
                    return ServiceResult<string>.Failure("Chỉ chấp nhận file ảnh (.jpg, .jpeg, .png, .gif, .webp)");
                }

                // Validate file size (max 5MB)
                if (imageFile.Length > 5 * 1024 * 1024)
                {
                    return ServiceResult<string>.Failure("Kích thước file không được vượt quá 5MB");
                }

                var uploadsFolder = Path.Combine(_environment.WebRootPath, "images", "products");
                Directory.CreateDirectory(uploadsFolder);
                
                // Generate filename using SKU
                var fileName = $"{sku}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                
                // Delete old file if exists and different from new file
                if (!string.IsNullOrEmpty(oldImageUrl))
                {
                    var oldFileName = Path.GetFileName(oldImageUrl);
                    var oldFilePath = Path.Combine(uploadsFolder, oldFileName);
                    if (System.IO.File.Exists(oldFilePath) && oldFileName != fileName)
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }
                
                // Delete existing file with same name
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                
                // Save new file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }
                
                return ServiceResult<string>.Success($"/images/products/{fileName}");
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Failure($"Lỗi upload ảnh: {ex.Message}");
            }
        }

        #endregion
    }
}