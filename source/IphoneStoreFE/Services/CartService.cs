using IphoneStoreBE.VModels;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text.Json;

namespace IphoneStoreFE.Services
{
    public interface ICartService
    {
        Task<List<CartGetVModel>> GetAllAsync(int userId);
        Task DeleteCartItem(int cartId);
    }

    public class CartService : ICartService
    {
        private readonly HttpClient _httpClient;

        public CartService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://localhost:7182/api/"); // Cấu hình base URL cho API
        }

        // 🟢 Truyền userId từ FE (session) để lấy giỏ hàng của người dùng
        public async Task<List<CartGetVModel>> GetAllAsync(int userId)
        {
            try
            {
                // Gọi đúng endpoint: /api/Cart/{userId}
                var response = await _httpClient.GetAsync($"Cart/{userId}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[CartService] Lỗi GET Cart/{userId}: {response.StatusCode}");
                    return new List<CartGetVModel>();  // Trả về danh sách trống nếu lỗi
                }

                var json = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(json))
                {
                    Console.WriteLine($"[CartService] No content returned from API.");
                    return new List<CartGetVModel>();  // Nếu không có dữ liệu, trả về danh sách trống
                }

                // Deserialize JSON và kiểm tra sự thành công
                var result = JsonConvert.DeserializeObject<ApiResponse<List<CartGetVModel>>>(json);

                if (result == null || !result.Success)
                {
                    Console.WriteLine($"[CartService] API response error: {result?.Message}");
                    return new List<CartGetVModel>();  // Trả về danh sách trống nếu API trả về thất bại
                }

                return result?.Data ?? new List<CartGetVModel>(); // Trả về dữ liệu nếu thành công
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CartService] Exception: {ex.Message}");
                return new List<CartGetVModel>();  // Trả về danh sách trống trong trường hợp lỗi
            }
        }

        // 🗑 Xóa sản phẩm khỏi giỏ hàng
        public async Task DeleteCartItem(int cartId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"Cart/{cartId}");
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[CartService] Failed to delete cart item {cartId}: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CartService] Exception when deleting cart item {cartId}: {ex.Message}");
            }
        }
    }

    // 🔹 API Response wrapper class
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}
