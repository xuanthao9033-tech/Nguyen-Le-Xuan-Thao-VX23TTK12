using IphoneStoreBE.Common.Models;
using IphoneStoreBE.VModels;
using Newtonsoft.Json;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace IphoneStoreFE.Services
{
    public class OrderService : IOrderService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OrderService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderService(HttpClient httpClient, ILogger<OrderService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        // Helper method to attach auth token to requests
        private void AttachAuthToken()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("Token");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _logger.LogDebug("🔑 Attached authorization token to request");
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
                _logger.LogWarning("⚠️ No auth token available for API request");
            }
        }

        public async Task<OrderResponse> CreateOrder(OrderCreateVModel model)
        {
            try
            {
                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Attach JWT token from session
                AttachAuthToken();

                // Ensure BaseAddress is set (defensive)
                if (_httpClient.BaseAddress == null)
                {
                    _logger.LogWarning("HttpClient.BaseAddress was null in OrderService.CreateOrder. Setting default to https://localhost:7182/api/");
                    _httpClient.BaseAddress = new Uri("https://localhost:7182/api/");
                }

                var response = await _httpClient.PostAsync("order/CreateFromCart", content);

                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("CreateOrder failed. Status: {Status}, Body: {Body}", response.StatusCode, responseBody);

                    // Try deserialize backend error object if any
                    string message = "Không thể tạo đơn hàng.";
                    try
                    {
                        var errObj = JsonConvert.DeserializeObject<dynamic>(responseBody);
                        if (errObj != null && errObj.message != null)
                            message = (string)errObj.message;
                    }
                    catch { }

                    return new OrderResponse
                    {
                        Success = false,
                        Message = message + " (HTTP " + (int)response.StatusCode + ")"
                    };
                }

                // Deserialize success response
                OrderResponse result = null;
                try
                {
                    result = JsonConvert.DeserializeObject<OrderResponse>(responseBody);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to deserialize CreateOrder response: {Body}", responseBody);
                }

                return result ?? new OrderResponse
                {
                    Success = false,
                    Message = "Lỗi khi xử lý phản hồi từ server."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return new OrderResponse
                {
                    Success = false,
                    Message = $"Lỗi: {ex.Message}"
                };
            }
        }

        public async Task<ResponseResult<PagedEntity<OrderGetVModel>>> GetAllOrdersAsync(int page = 1, int pageSize = 20)
        {
            try
            {
                _logger.LogInformation("🔍 GetAllOrdersAsync - Đang gọi API lấy tất cả đơn hàng - Trang: {Page}, Kích thước trang: {PageSize}", page, pageSize);
                
                // Thêm timestamp để tránh cache
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                
                // Attach auth token
                AttachAuthToken();
                
                var response = await _httpClient.GetAsync($"order/all?page={page}&pageSize={pageSize}&_ts={timestamp}");
                
                _logger.LogInformation("📡 API Response Status: {StatusCode} ({ReasonPhrase})", 
                    response.StatusCode, 
                    response.ReasonPhrase);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("⚠️ GetAllOrders thất bại - Status: {Status}, Lỗi: {Body}", 
                        response.StatusCode, 
                        errorBody);
                    
                    // Xử lý các loại lỗi cụ thể
                    return response.StatusCode switch
                    {
                        System.Net.HttpStatusCode.NotFound => 
                            ResponseResult<PagedEntity<OrderGetVModel>>.Fail("Không tìm thấy endpoint API. Vui lòng kiểm tra backend đang chạy."),
                        
                        System.Net.HttpStatusCode.Unauthorized => 
                            ResponseResult<PagedEntity<OrderGetVModel>>.Fail("Chưa đăng nhập. Vui lòng đăng nhập lại."),
                        
                        System.Net.HttpStatusCode.Forbidden => 
                            ResponseResult<PagedEntity<OrderGetVModel>>.Fail("Bạn không có quyền xem danh sách đơn hàng. Vui lòng đăng nhập với tài khoản Admin."),
                        
                        _ => ResponseResult<PagedEntity<OrderGetVModel>>.Fail($"Không thể tải danh sách đơn hàng. Mã lỗi: {(int)response.StatusCode}")
                    };
                }

                var json = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("✅ API Response thành công - Độ dài dữ liệu: {Length} bytes", json?.Length ?? 0);
                
                var result = JsonConvert.DeserializeObject<ResponseResult<PagedEntity<OrderGetVModel>>>(json);
                
                if (result != null)
                {
                    _logger.LogInformation("📊 Kết quả - Success: {Success}, Có dữ liệu: {HasData}", 
                        result.Success, 
                        result.Data != null);
                    
                    if (result.Data != null)
                    {
                        _logger.LogInformation("📦 Chi tiết dữ liệu - Số đơn hàng: {Count}, Tổng: {Total}, Trang: {Page}/{TotalPages}",
                            result.Data.Items?.Count ?? 0,
                            result.Data.TotalItems,
                            result.Data.PageIndex,
                            result.Data.TotalPages);
                    }
                }

                return result ?? ResponseResult<PagedEntity<OrderGetVModel>>.Fail("Dữ liệu trả về không hợp lệ. Vui lòng thử lại.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "❌ Lỗi kết nối đến Backend API - Page: {Page}, PageSize: {PageSize}", page, pageSize);
                return ResponseResult<PagedEntity<OrderGetVModel>>.Fail("Không thể kết nối đến máy chủ. Vui lòng kiểm tra backend đang chạy trên port 7182.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Lỗi không xác định khi lấy danh sách đơn hàng - Page: {Page}, PageSize: {PageSize}", page, pageSize);
                return ResponseResult<PagedEntity<OrderGetVModel>>.Fail($"Lỗi: {ex.Message}");
            }
        }

        public async Task<ResponseResult<OrderGetVModel>> GetOrderByIdAsync(int id)
        {
            try
            {
                // Attach auth token
                AttachAuthToken();
                
                // Thêm timestamp để tránh cache
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var response = await _httpClient.GetAsync($"order/{id}?_ts={timestamp}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("❌ GetOrderById failed. Status: {Status}, Content: {Content}", 
                        response.StatusCode, errorContent);
                    return ResponseResult<OrderGetVModel>.Fail($"Không thể tải chi tiết đơn hàng (HTTP {(int)response.StatusCode})");
                }

                var json = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("📦 GetOrderById response: {Json}", json);
                var result = JsonConvert.DeserializeObject<ResponseResult<OrderGetVModel>>(json);

                return result ?? ResponseResult<OrderGetVModel>.Fail("Dữ liệu trả về không hợp lệ");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting order by id {Id}", id);
                return ResponseResult<OrderGetVModel>.Fail($"Lỗi: {ex.Message}");
            }
        }

        public async Task<ResponseResult<PagedEntity<OrderGetVModel>>> GetOrdersByUserIdAsync(int userId, int page = 1, int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("🔍 GetOrdersByUserIdAsync - UserId: {UserId}, Page: {Page}, PageSize: {PageSize}", 
                    userId, page, pageSize);
                
                // Attach auth token
                AttachAuthToken();
                
                // Thêm timestamp để tránh cache
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var response = await _httpClient.GetAsync($"order/user/{userId}?page={page}&pageSize={pageSize}&_ts={timestamp}");
                
                var statusCode = (int)response.StatusCode;
                _logger.LogInformation("📡 GetOrdersByUserIdAsync response status: {Status}", statusCode);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("❌ GetOrdersByUserIdAsync failed. Status: {Status}, Content: {Content}",
                        statusCode, errorContent);
                    return ResponseResult<PagedEntity<OrderGetVModel>>.Fail(
                        $"Không thể tải danh sách đơn hàng (HTTP {statusCode})");
                }

                var json = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("📦 GetOrdersByUserIdAsync response: {Json}", json);
                
                var result = JsonConvert.DeserializeObject<ResponseResult<PagedEntity<OrderGetVModel>>>(json);
                
                if (result == null)
                {
                    _logger.LogError("❌ Failed to deserialize response from GetOrdersByUserIdAsync");
                    return ResponseResult<PagedEntity<OrderGetVModel>>.Fail("Lỗi xử lý dữ liệu từ server");
                }
                
                if (!result.Success || result.Data == null)
                {
                    _logger.LogWarning("⚠️ GetOrdersByUserIdAsync returned unsuccessful result: {Message}", result.Message);
                    return result;
                }
                
                _logger.LogInformation("✅ GetOrdersByUserIdAsync successful - Found {Count} orders for user {UserId}",
                    result.Data.Items?.Count ?? 0, userId);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Exception in GetOrdersByUserIdAsync for user {UserId}", userId);
                return ResponseResult<PagedEntity<OrderGetVModel>>.Fail($"Lỗi: {ex.Message}");
            }
        }

        public async Task<ResponseResult> UpdateOrderStatusAsync(int orderId, string status)
        {
            try
            {
                // Attach auth token
                AttachAuthToken();
                
                var json = JsonConvert.SerializeObject(new { OrderStatus = status });
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"order/status/{orderId}", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("❌ UpdateOrderStatus failed. Status: {Status}, Content: {Content}",
                        response.StatusCode, errorContent);
                    return ResponseResult.Fail($"Không thể cập nhật trạng thái đơn hàng (HTTP {(int)response.StatusCode})");
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("📦 UpdateOrderStatus response: {Json}", responseJson);
                var result = JsonConvert.DeserializeObject<ResponseResult>(responseJson);

                return result ?? ResponseResult.Fail("Dữ liệu trả về không hợp lệ");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status for order {OrderId}", orderId);
                return ResponseResult.Fail($"Lỗi: {ex.Message}");
            }
        }
    }
}
