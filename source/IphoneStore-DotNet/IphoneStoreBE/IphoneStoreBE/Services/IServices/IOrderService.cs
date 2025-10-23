using IphoneStoreBE.Common.Models;
using IphoneStoreBE.VModels;

namespace IphoneStoreBE.Services.IServices
{
    public interface IOrderService  
    {
        Task<ResponseResult> ProcessPaymentAsync(OrderCreateVModel model, HttpContext httpContext);
        Task<ResponseResult<OrderGetVModel>> GetOrderByIdAsync(int id);
        Task<ResponseResult<PagedEntity<OrderGetVModel>>> GetOrdersByUserIdAsync(int userId, int page = 1, int pageSize = 10);
        Task<ResponseResult<PagedEntity<OrderGetVModel>>> GetAllOrdersAsync(int page = 1, int pageSize = 20);
        Task<ResponseResult> UpdateOrderStatusAsync(int orderId, string status);
        Task<ResponseResult> CancelOrderAsync(int orderId, int userId);
    }
}       