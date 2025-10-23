using IphoneStoreBE.VModels;
using IphoneStoreBE.Common.Models;

namespace IphoneStoreFE.Services
{
    public interface IOrderService
    {
        Task<OrderResponse> CreateOrder(OrderCreateVModel model);

        Task<IphoneStoreBE.Common.Models.ResponseResult<PagedEntity<OrderGetVModel>>> GetAllOrdersAsync(int page = 1, int pageSize = 20);
        Task<IphoneStoreBE.Common.Models.ResponseResult<OrderGetVModel>> GetOrderByIdAsync(int id);
        Task<IphoneStoreBE.Common.Models.ResponseResult<PagedEntity<OrderGetVModel>>> GetOrdersByUserIdAsync(int userId, int page = 1, int pageSize = 10);
        Task<IphoneStoreBE.Common.Models.ResponseResult> UpdateOrderStatusAsync(int orderId, string status);
    }

    public class OrderResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
    }
}
