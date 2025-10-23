using IphoneStoreBE.VModels;
using IphoneStoreFE.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using IphoneStoreFE.Services;
using Microsoft.Extensions.Configuration;

namespace IphoneStoreFE.Controllers
{
    public class OrderController : Controller
    {
        private readonly HttpClient _client;
        private readonly ILogger<OrderController> _logger;
        private readonly ICartService _cartService;
        private readonly IOrderService _order_service;
        private readonly string _backendBaseUrl;

        public OrderController(
            IHttpClientFactory httpClientFactory,
            ILogger<OrderController> logger,
            ICartService cartService,
            IOrderService orderService,
            IConfiguration configuration
        )
        {
            _client = httpClientFactory.CreateClient("IphoneStoreAPI");
            _logger = logger;
            _cartService = cartService;
            _order_service = orderService;
            _backendBaseUrl = configuration["BackendBaseUrl"] ?? "https://localhost:7182";
        }

        // ============================================================
        // 🟢 Danh sách đơn hàng của người dùng
        // ============================================================
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                _logger.LogWarning("⚠️ Session UserId is null — redirecting to Home/Index");
                return RedirectToAction("Index", "Home");
            }

            try
            {
                // Thêm timestamp để tránh cache
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var apiUrl = $"order/user/{userId}?page={page}&pageSize={pageSize}&_ts={timestamp}";
                _logger.LogInformation("🔹 Fetching orders from API: {Url}", apiUrl);

                // Thêm token xác thực để đảm bảo request được ủy quyền
                var token = HttpContext.Session.GetString("Token");
                if (!string.IsNullOrEmpty(token))
                {
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    _logger.LogInformation("🔑 Added authorization token to request");
                }

                var response = await _client.GetAsync(apiUrl);
                _logger.LogInformation("🟢 API Response Status: {StatusCode}", response.StatusCode);

                var responseContent = await response.Content.ReadAsStringAsync();
                // Thêm debug info để dễ dàng phát hiện vấn đề
                ViewBag.ApiUrl = apiUrl;
                ViewBag.StatusCode = (int)response.StatusCode;
                ViewBag.ResponseContent = responseContent.Length > 200 
                    ? responseContent.Substring(0, 200) + "..." 
                    : responseContent;

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("❌ Failed to load orders. StatusCode: {StatusCode}, Content: {Content}", 
                        response.StatusCode, responseContent);
                    
                    // Nếu 404, có thể user chưa có đơn hàng nào
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        ViewBag.Info = "Bạn chưa có đơn hàng nào.";
                        ViewBag.BackendBaseUrl = _backendBaseUrl;
                        return View(new List<OrderGetVModel>());
                    }
                    
                    ViewBag.Error = $"Không thể tải danh sách đơn hàng. (HTTP {(int)response.StatusCode})";
                    ViewBag.BackendBaseUrl = _backendBaseUrl;
                    return View(new List<OrderGetVModel>());
                }
                    
                var json = responseContent;
                _logger.LogInformation("📦 Raw API Response: {Json}", json);

                // Deserialize using backend ResponseResult / PagedEntity to avoid ambiguous FE types
                ResponseResult<PagedEntity<OrderGetVModel>> result;
                try
                {
                    result = JsonConvert.DeserializeObject<ResponseResult<PagedEntity<OrderGetVModel>>>(json);
                    if (result == null)
                    {
                        _logger.LogError("❌ Failed to deserialize response: {Response}", json);
                        ViewBag.Error = "Lỗi khi xử lý dữ liệu từ server.";
                        return View(new List<OrderGetVModel>());
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Exception deserializing API response: {Response}", json);
                    ViewBag.Error = "Lỗi phân tích dữ liệu từ server.";
                    ViewBag.Exception = ex.Message;
                    return View(new List<OrderGetVModel>());
                }

                if (result?.Data?.Items == null || !result.Success)
                {
                    _logger.LogWarning("⚠️ No orders found or failed: {Message}", result?.Message);
                    ViewBag.Info = result?.Message ?? "Bạn chưa có đơn hàng nào.";
                    ViewBag.BackendBaseUrl = _backendBaseUrl;
                    return View(new List<OrderGetVModel>());
                }

                var orders = result.Data.Items;
                _logger.LogInformation("✅ Loaded {Count} orders from API for UserId {UserId}.", orders.Count, userId);

                if (orders.Count > 0)
                {
                    _logger.LogInformation("📦 First order: ID={Id}, Code={Code}, Date={Date}, Status={Status}", 
                        orders[0].Id, orders[0].OrderCode, orders[0].OrderDate, orders[0].OrderStatus);
                }

                // Pass pagination info to view
                ViewBag.CurrentPage = result.Data.PageIndex;
                ViewBag.TotalPages = result.Data.TotalPages;
                ViewBag.HasPrevious = result.Data.HasPreviousPage;
                ViewBag.HasNext = result.Data.HasNextPage;
                ViewBag.BackendBaseUrl = _backendBaseUrl;
                ViewBag.UserId = userId;

                // Thêm thông báo thành công nếu có TempData
                if (TempData["Success"] == null && orders.Any())
                {
                    TempData["Success"] = "Danh sách đơn hàng đã được cập nhật.";
                }

                return View(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Exception when loading orders for UserId {UserId}", userId);
                ViewBag.Error = "Đã xảy ra lỗi khi tải danh sách đơn hàng.";
                ViewBag.Exception = ex.ToString();
                ViewBag.BackendBaseUrl = _backendBaseUrl;
                ViewBag.UserId = userId;
                return View(new List<OrderGetVModel>());
            }
        }

        // ============================================================
        // 🟦 Chi tiết đơn hàng
        // ============================================================
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var url = $"order/{id}";
                _logger.LogInformation("🔍 Fetching order detail from: {Url}", url);
                
                // Thêm token xác thực
                var token = HttpContext.Session.GetString("Token");
                if (!string.IsNullOrEmpty(token))
                {
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _client.GetAsync(url);
                _logger.LogInformation("🟢 Detail API Response: {Status}", response.StatusCode);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("❌ Failed to load order detail. Status: {Status}, Content: {Content}", 
                        response.StatusCode, errorContent);
                    TempData["Error"] = "Không thể tải chi tiết đơn hàng.";
                    return RedirectToAction("Index");
                }

                var json = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("📜 Order Detail JSON: {Json}", json);

                var result = JsonConvert.DeserializeObject<ResponseResult<OrderGetVModel>>(json);

                if (result?.Data == null || !result.Success)
                {
                    TempData["Error"] = result?.Message ?? "Đơn hàng không tồn tại.";
                    return RedirectToAction("Index");
                }

                ViewBag.BackendBaseUrl = _backendBaseUrl;
                _logger.LogInformation("✅ Loaded order detail successfully.");
                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Exception while loading order detail (ID={Id})", id);
                TempData["Error"] = "Lỗi khi tải chi tiết đơn hàng.";
                return RedirectToAction("Index");
            }
        }

        // ============================================================
        // 🟢 Trang tạo đơn hàng (GET)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                _logger.LogWarning("⚠️ User not logged in, redirecting to login");
                TempData["Error"] = "Vui lòng đăng nhập để đặt hàng.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var cartItems = await _cartService.GetAllAsync(userId.Value);
                
                if (cartItems == null || !cartItems.Any())
                {
                    _logger.LogWarning("⚠️ Cart is empty for user {UserId}", userId);
                    TempData["Error"] = "Giỏ hàng của bạn đang trống.";
                    return RedirectToAction("Index", "Cart");
                }

                _logger.LogInformation("✅ User {UserId} accessing order creation page with {Count} items", userId, cartItems.Count);
                
                ViewBag.CartItems = cartItems;
                ViewBag.UserId = userId;
                ViewBag.BackendBaseUrl = _backendBaseUrl;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Exception when loading order creation page for user {UserId}", userId);
                TempData["Error"] = "Đã xảy ra lỗi. Vui lòng thử lại.";
                return RedirectToAction("Index", "Cart");
            }
        }

        // ============================================================
        // 🟡 Hủy đơn hàng
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var token = HttpContext.Session.GetString("Token");
                _logger.LogInformation("🔸 Canceling order ID={Id}", id);

                var request = new HttpRequestMessage(HttpMethod.Put, $"order/cancel/{id}");
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Đơn hàng đã được hủy thành công.";
                    _logger.LogInformation("✅ Order {Id} canceled successfully.", id);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = "Không thể hủy đơn hàng.";
                    _logger.LogError("❌ Failed to cancel order {Id}. Status: {Status}, Error: {Error}", 
                        id, response.StatusCode, errorContent);
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Exception while canceling order {Id}", id);
                TempData["Error"] = "Lỗi khi hủy đơn hàng.";
                return RedirectToAction("Index");
            }
        }

        // ============================================================
        // 🟣 Tạo đơn hàng mới (POST)
        // ============================================================
        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderViewModel model)
        {
            try
            {
                _logger.LogInformation("🔹 Creating order for user via backend API");

                // Kiểm tra dữ liệu đầu vào
                if (string.IsNullOrWhiteSpace(model.FullName) || 
                    string.IsNullOrWhiteSpace(model.Phone) || 
                    string.IsNullOrWhiteSpace(model.Address))
                {
                    _logger.LogWarning("⚠️ Invalid order data: Missing required fields");
                    return Json(new { 
                        success = false, 
                        message = "Vui lòng nhập đầy đủ thông tin giao hàng (họ tên, số điện thoại, địa chỉ)."
                    });
                }

                if (model.CartIds == null || !model.CartIds.Any())
                {
                    _logger.LogWarning("⚠️ Invalid order data: No cart items");
                    return Json(new { 
                        success = false, 
                        message = "Giỏ hàng trống, không thể đặt hàng."
                    });
                }

                var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
                if (userId <= 0)
                {
                    _logger.LogWarning("⚠️ User not authenticated");
                    return Json(new { 
                        success = false, 
                        message = "Bạn cần đăng nhập để đặt hàng." 
                    });
                }

                var token = HttpContext.Session.GetString("Token");
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("⚠️ No auth token found");
                    return Json(new { 
                        success = false, 
                        message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại." 
                    });
                }

                // Ánh xạ từ CreateOrderViewModel sang OrderCreateVModel
                var payload = new OrderCreateVModel
                {
                    UserId = userId,
                    Recipient = model.FullName,            // Map FullName sang Recipient
                    PhoneNumber = model.Phone,             // Map Phone sang PhoneNumber
                    AddressDetailRecipient = model.Address, // Map Address sang AddressDetailRecipient
                    City = model.City ?? "",
                    District = model.District ?? "",
                    Ward = model.Ward ?? "",
                    PaymentMethod = model.PaymentMethod,
                    ShippingPrice = model.PaymentMethod?.ToUpper() == "BANK" ? 30000 : 0
                };

                _logger.LogInformation("📦 Order payload: UserId={UserId}, Recipient={Recipient}, Phone={Phone}, Address={Address}, Payment={Payment}", 
                    payload.UserId, payload.Recipient, payload.PhoneNumber, payload.AddressDetailRecipient, payload.PaymentMethod);

                // Thiết lập token xác thực
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _logger.LogInformation("🔑 Using auth token for API request");

                var jsonPayload = JsonConvert.SerializeObject(payload);
                _logger.LogDebug("📦 Sending JSON payload: {Json}", jsonPayload);

                // Gửi request đến backend API
                var response = await _client.PostAsJsonAsync("order/CreateFromCart", payload);
                var responseBody = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("📡 Backend CreateFromCart response: {Status} {Body}", response.StatusCode, responseBody);

                if (!response.IsSuccessStatusCode)
                {
                    string errorMessage;
                    try
                    {
                        var err = JsonConvert.DeserializeObject<dynamic>(responseBody);
                        errorMessage = err?.message ?? "Không thể tạo đơn hàng.";
                        _logger.LogWarning("⚠️ Order creation failed: {Message}", errorMessage);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Error parsing API error response");
                        errorMessage = $"Không thể tạo đơn hàng. Lỗi máy chủ ({(int)response.StatusCode})";
                    }
                    return Json(new { success = false, message = errorMessage });
                }

                // Xử lý phản hồi thành công
                dynamic resultObj;
                try
                {
                    _logger.LogInformation("📝 Parsing response body: {ResponseBody}", responseBody);
                    resultObj = JsonConvert.DeserializeObject<dynamic>(responseBody);
                    _logger.LogInformation("📝 Parsed result: Success={Success}, Message={Message}", (bool?)resultObj?.success, (string?)resultObj?.message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error deserializing successful response. Response body: {ResponseBody}", responseBody);
                    return Json(new { 
                        success = false, 
                        message = "Lỗi xử lý phản hồi từ máy chủ." 
                    });
                }

                bool success = resultObj?.success == true;
                string message = resultObj?.message ?? "Đặt hàng thành công!";
                int? orderId = null;

                if (success)
                {
                    _logger.LogInformation("✅ Order created successfully: {Message}", message);
                    
                    // Lưu thông tin đơn hàng mới vào TempData để hiển thị thông báo
                    if (resultObj?.data != null)
                    {
                        try
                        {
                            orderId = (int?)resultObj.data.id;
                            string orderCode = (string)resultObj.data.orderCode;
                            TempData["NewOrderId"] = orderId;
                            TempData["NewOrderCode"] = orderCode;
                            TempData["Success"] = $"Đặt hàng thành công! Mã đơn hàng: {orderCode}";
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to extract order details from response");
                        }
                    }
                    
                    // Xóa giỏ hàng sau khi đặt hàng thành công
                    if (model.CartIds != null && model.CartIds.Any())
                    {
                        _logger.LogInformation("🛒 Clearing {Count} cart items after successful order", model.CartIds.Count);
                        foreach (var cartId in model.CartIds)
                        {
                            await _cartService.DeleteCartItem(cartId);
                        }
                    }
                }

                return Json(new { success = success, message = message, data = resultObj?.data, orderId = orderId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Exception creating order via backend API");
                return Json(new { 
                    success = false, 
                    message = "Đã xảy ra lỗi khi đặt hàng. Vui lòng thử lại sau." 
                });
            }
        }
    }
}
