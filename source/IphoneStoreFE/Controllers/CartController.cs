using IphoneStoreBE.Common.Models;
using IphoneStoreBE.VModels;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace IphoneStoreFE.Controllers
{
    public class CartController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly string _backendBaseUrl;

        public CartController(IHttpClientFactory httpClientFactory, IHttpContextAccessor accessor, IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7182/api/");

            // Gắn token nếu có
            var token = accessor.HttpContext?.Session.GetString("Token");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }

            _backendBaseUrl = configuration["BackendBaseUrl"] ?? "https://localhost:7182";
        }

        // 🛒 [GET] /Cart/Index — Hiển thị giỏ hàng
        public async Task<IActionResult> Index()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                // Nếu session hết hạn, chuyển hướng về trang đăng nhập
                TempData["Message"] = "Vui lòng đăng nhập để xem giỏ hàng.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var res = await _httpClient.GetAsync($"Cart/{userId}");
                if (!res.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Không thể tải giỏ hàng. Vui lòng thử lại sau.";
                    ViewBag.BackendBaseUrl = _backendBaseUrl;
                    return View(new List<CartGetVModel>());
                }

                var result = await res.Content.ReadFromJsonAsync<ResponseResult<List<CartGetVModel>>>(_jsonOptions);
                if (result?.Data == null)
                {
                    ViewBag.Error = "Giỏ hàng của bạn đang trống.";
                    ViewBag.BackendBaseUrl = _backendBaseUrl;
                    return View(new List<CartGetVModel>());
                }

                ViewBag.BackendBaseUrl = _backendBaseUrl;
                return View(result.Data);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Đã xảy ra lỗi: {ex.Message}";
                ViewBag.BackendBaseUrl = _backendBaseUrl;
                return View(new List<CartGetVModel>());
            }
        }

        // 🗑 [DELETE] /Cart/Clear/{userId} — Xóa toàn bộ giỏ hàng
        [HttpDelete]
        public async Task<IActionResult> Clear(int userId)
        {
            try
            {
                var res = await _httpClient.DeleteAsync($"Cart/Clear/{userId}");
                return Json(new { 
                    success = res.IsSuccessStatusCode,
                    message = res.IsSuccessStatusCode ? "Đã xóa toàn bộ giỏ hàng." : "Không thể xóa giỏ hàng."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // 🗑 [DELETE] /Cart/{id} — Xóa 1 sản phẩm
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var res = await _httpClient.DeleteAsync($"Cart/{id}");
                return Json(new { 
                    success = res.IsSuccessStatusCode,
                    message = res.IsSuccessStatusCode ? "Đã xóa sản phẩm khỏi giỏ hàng." : "Không thể xóa sản phẩm."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // 🔄 [PUT] /Cart/{id} — Cập nhật số lượng sản phẩm
        [HttpPut]
        public async Task<IActionResult> UpdateQuantity(int id, [FromBody] int quantity)
        {
            try
            {
                var res = await _httpClient.PutAsJsonAsync($"Cart/{id}", quantity);
                return Json(new { 
                    success = res.IsSuccessStatusCode,
                    message = res.IsSuccessStatusCode ? "Đã cập nhật số lượng." : "Không thể cập nhật số lượng."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // ➕ [POST] /Cart — Thêm sản phẩm vào giỏ hàng
        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] CartGetVModel model)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng." });
            }

            try
            {
                model.UserId = userId.Value;

                var res = await _httpClient.PostAsJsonAsync("Cart", model);
                var result = await res.Content.ReadFromJsonAsync<ResponseResult>(_jsonOptions);

                if (res.IsSuccessStatusCode && result?.Success == true)
                {
                    return Json(new { success = true, message = "Đã thêm sản phẩm vào giỏ hàng." });
                }
                else
                {
                    return Json(new { success = false, message = result?.Message ?? "Không thể thêm sản phẩm vào giỏ hàng." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
    }
}
