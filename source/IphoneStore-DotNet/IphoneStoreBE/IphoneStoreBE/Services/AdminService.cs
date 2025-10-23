using IphoneStoreBE.Common.Models;
using IphoneStoreBE.Context;
using IphoneStoreBE.Services.IServices;
using IphoneStoreBE.VModels;
using Microsoft.EntityFrameworkCore;

namespace IphoneStoreBE.Services
{
    public class AdminService : IAdminService
    {
        private readonly IphoneStoreContext _context;

        public AdminService(IphoneStoreContext context)
        {
            _context = context;
        }

        public async Task<ResponseResult<AdminStatisticsVModel>> GetStatisticsAsync()
        {
            try
            {
                var stats = new AdminStatisticsVModel
                {
                    TotalOrders = await _context.Orders.CountAsync(),
                    TotalProducts = await _context.Products.CountAsync(p => p.IsActive == true),
                    TotalCustomers = await _context.Users.CountAsync(u => u.RoleId != 1), // Không tính Admin
                    TotalRevenue = await _context.Orders
                        .Where(o => o.OrderStatus == "Đã giao thành công")
                        .SumAsync(o => o.Total ?? 0),
                    PendingOrders = await _context.Orders
                        .CountAsync(o => o.OrderStatus == "Chờ xác nhận"),
                    ProcessingOrders = await _context.Orders
                        .CountAsync(o => o.OrderStatus == "Đang chuẩn bị" || o.OrderStatus == "Đang giao hàng"),
                    CompletedOrders = await _context.Orders
                        .CountAsync(o => o.OrderStatus == "Đã giao thành công"),
                    CancelledOrders = await _context.Orders
                        .CountAsync(o => o.OrderStatus == "Đã hủy")
                };

                return ResponseResult<AdminStatisticsVModel>.Ok(stats, "Lấy thống kê thành công!");
            }
            catch (Exception ex)
            {
                return ResponseResult<AdminStatisticsVModel>.Fail($"Lỗi khi lấy thống kê: {ex.Message}");
            }
        }

        public async Task<ResponseResult<List<OrderGetVModel>>> GetRecentOrdersAsync(int count = 10)
        {
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderAdd)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                    .OrderByDescending(o => o.CreatedDate)
                    .Take(count)
                    .Select(o => new OrderGetVModel
                    {
                        Id = o.Id,
                        OrderCode = o.OrderCode,
                        OrderDate = o.OrderDate,
                        Total = o.Total ?? 0,
                        ShippingPrice = o.ShippingPrice,
                        PaymentMethod = o.PaymentMethod,
                        OrderStatus = o.OrderStatus,
                        UserName = o.User != null ? o.User.UserName : "N/A",
                        OrderDetails = o.OrderDetails != null ? o.OrderDetails.Select(od => new OrderDetailGetVModel
                        {
                            ProductId = od.ProductId ?? 0,
                            ProductName = od.Product != null ? od.Product.ProductName : "N/A",
                            Quantity = od.Quantity,
                            Price = od.Price
                        }).ToList() : new List<OrderDetailGetVModel>()
                    })
                    .ToListAsync();

                return ResponseResult<List<OrderGetVModel>>.Ok(orders, "Lấy danh sách đơn hàng thành công!");
            }
            catch (Exception ex)
            {
                return ResponseResult<List<OrderGetVModel>>.Fail($"Lỗi khi lấy danh sách đơn hàng: {ex.Message}");
            }
        }
    }
}