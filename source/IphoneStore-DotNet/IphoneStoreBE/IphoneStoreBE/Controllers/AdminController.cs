using IphoneStoreBE.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IphoneStoreBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IOrderService _orderService;

        public AdminController(IAdminService adminService, IOrderService orderService)
        {
            _adminService = adminService;
            _orderService = orderService;
        }

        // 📊 Lấy thống kê tổng quan
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var result = await _adminService.GetStatisticsAsync();
            return Ok(result);
        }

        // 📋 Lấy đơn hàng gần đây
        [HttpGet("recent-orders")]
        public async Task<IActionResult> GetRecentOrders([FromQuery] int count = 10)
        {
            var result = await _adminService.GetRecentOrdersAsync(count);
            return Ok(result);
        }

        // 📦 Lấy tất cả đơn hàng với phân trang
        [HttpGet("orders")]
        public async Task<IActionResult> GetAllOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _orderService.GetAllOrdersAsync(page, pageSize);
            return Ok(result);
        }

        // 📄 Lấy chi tiết đơn hàng
        [HttpGet("orders/{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var result = await _orderService.GetOrderByIdAsync(id);
            return Ok(result);
        }

        // ✅ Cập nhật trạng thái đơn hàng
        [HttpPut("orders/{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusVModel model)
        {
            var result = await _orderService.UpdateOrderStatusAsync(id, model.OrderStatus);
            return Ok(result);
        }
    }

    // Model để nhận status từ client
    public class UpdateOrderStatusVModel
    {
        public string OrderStatus { get; set; } = string.Empty;
    }
}