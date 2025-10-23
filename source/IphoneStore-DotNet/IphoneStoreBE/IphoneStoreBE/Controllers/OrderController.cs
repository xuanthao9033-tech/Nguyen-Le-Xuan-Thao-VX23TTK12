using IphoneStoreBE.Common.Models;
using IphoneStoreBE.Context;
using IphoneStoreBE.Entities;
using IphoneStoreBE.Mappings;
using IphoneStoreBE.Services.IServices;
using IphoneStoreBE.VModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IphoneStoreBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IphoneStoreContext _context;
        private readonly ILogger<OrderController> _logger;
        private readonly IOrderService _orderService;

        public OrderController(IphoneStoreContext context, ILogger<OrderController> logger, IOrderService orderService)
        {
            _context = context;
            _logger = logger;
            _orderService = orderService;
        }

        // ============================================================
        // 🟢 API: Tạo đơn hàng từ giỏ hàng
        // ============================================================
        [HttpPost("CreateFromCart")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CreateFromCart([FromBody] OrderCreateVModel model)
        {
            _logger.LogInformation("🚀 ============ CREATE ORDER START ============");
            _logger.LogInformation("   Timestamp: {Time}", DateTime.UtcNow);
            _logger.LogInformation("   Request Model: {@Model}", model);

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Take userId from JWT claims
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                _logger.LogInformation("🔑 User ID Claim: {Claim}", userIdClaim);

                if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId) || userId <= 0)
                {
                    _logger.LogWarning("⚠️ Missing or invalid user id in claims");
                    return Ok(new { success = false, message = "Không xác thực được người dùng." });
                }

                _logger.LogInformation("✅ Authenticated UserId: {UserId}", userId);

                // Validate user existence
                var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
                _logger.LogInformation("👤 User exists check: {Exists}", userExists);

                if (!userExists)
                {
                    _logger.LogWarning("⚠️ User {UserId} does not exist", userId);
                    return Ok(new { success = false, message = $"Người dùng với ID {userId} không tồn tại." });
                }

                // Load cart by claims user
                _logger.LogInformation("🛒 Loading cart items for UserId: {UserId}", userId);
                var cartItems = await _context.Carts
                    .Include(c => c.Product)
                    .Where(c => c.UserId == userId && c.IsActive == true)
                    .ToListAsync();

                _logger.LogInformation("📦 Cart items count: {Count}", cartItems.Count);

                if (!cartItems.Any())
                {
                    _logger.LogWarning("⚠️ Cart is empty for user {UserId}", userId);
                    return Ok(new { success = false, message = "Giỏ hàng trống, không thể tạo đơn hàng." });
                }

                // Log cart details
                foreach (var item in cartItems)
                {
                    _logger.LogInformation("  📌 Cart Item: ProductId={ProductId}, Quantity={Quantity}, ProductName={Name}", 
                        item.ProductId, item.Quantity, item.Product?.ProductName ?? "NULL");
                }

                // Validate all cart items have products
                var invalidItems = cartItems.Where(c => c.Product == null).ToList();
                if (invalidItems.Any())
                {
                    _logger.LogError("❌ Found {Count} cart items with null products", invalidItems.Count);
                    return Ok(new
                    {
                        success = false,
                        message = "Giỏ hàng có sản phẩm không hợp lệ. Vui lòng kiểm tra lại."
                    });
                }

                _logger.LogInformation("🏠 Creating order address...");
                // 🏠 Tạo địa chỉ giao hàng
                var orderAddress = new OrderAddress
                {
                    Recipient = model.Recipient ?? "",
                    PhoneNumber = model.PhoneNumber ?? "",
                    AddressDetailRecipient = model.AddressDetailRecipient ?? "",
                    City = model.City ?? "",
                    District = model.District ?? "",
                    Ward = model.Ward ?? "",
                    UserId = userId,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                // 💾 Lưu địa chỉ trước để có Id (fix lỗi FK)
                _context.OrderAddresses.Add(orderAddress);
                await _context.SaveChangesAsync();

                _logger.LogInformation("🛍️ Creating order...");
                var order = new Order
                {
                    UserId = userId,
                    OrderCode = "ORD" + DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                    OrderDate = DateTime.Now,
                    Total = 0,
                    PaymentMethod = model.PaymentMethod ?? "COD",
                    OrderStatus = "Chờ xác nhận",
                    ShippingPrice = model.ShippingPrice,
                    OrderAddId = orderAddress.Id,
                    CreatedDate = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                decimal totalAmount = 0;
                _logger.LogInformation("📊 Adding order details...");

                foreach (var item in cartItems)
                {
                    if (item.Product == null) continue;

                    _context.OrderDetails.Add(new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Product.Price,
                        CreatedDate = DateTime.UtcNow,
                        IsActive = true
                    });

                    totalAmount += item.Quantity * item.Product.Price;

                    _logger.LogInformation("  ➕ Added ProductId={ProductId}, Quantity={Quantity}, Price={Price}", 
                        item.ProductId, item.Quantity, item.Product.Price);
                }

                totalAmount += model.ShippingPrice;
                order.Total = totalAmount;

                _logger.LogInformation("💰 Total Amount: {TotalAmount}", totalAmount);

                await _context.SaveChangesAsync();

                _logger.LogInformation("🧺 Clearing cart items...");
                _context.Carts.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                _logger.LogInformation("✅ Order created successfully! OrderCode: {OrderCode}", order.OrderCode);
                return Ok(new
                {
                    success = true,
                    message = $"Đặt hàng thành công! Mã đơn hàng: {order.OrderCode}",
                    data = new
                    {
                        id = order.Id,
                        orderCode = order.OrderCode,
                        userId = order.UserId,
                        total = order.Total,
                        orderStatus = order.OrderStatus
                    }
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "❌ Error creating order: {Message}", ex.Message);
                _logger.LogError("❌ Stack trace: {StackTrace}", ex.StackTrace);
                return Ok(new { success = false, message = $"Lỗi máy chủ khi tạo đơn hàng: {ex.Message}" });
            }
                finally
                {
                    _logger.LogInformation("🚀 ============ CREATE ORDER END ============");
                }
            });
        }

        // ============================================================
        // 📦 Lấy danh sách đơn hàng của 1 user
        // ============================================================
        [HttpGet("user/{userId:int}")]           // <-- Specific route trước
        public async Task<IActionResult> GetOrdersByUser(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("🔍 GetOrdersByUser - UserId: {UserId}, Page: {Page}, PageSize: {PageSize}", userId, page, pageSize);

                var result = await _orderService.GetOrdersByUserIdAsync(userId, page, pageSize);

                if (!result.Success)
                {
                    _logger.LogWarning("⚠️ Failed to get orders: {Message}", result.Message);
                    return BadRequest(result);
                }

                _logger.LogInformation("✅ Retrieved {Count} orders for user {UserId}", result.Data?.Items?.Count ?? 0, userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error GetOrdersByUser {UserId}", userId);
                return StatusCode(500, ResponseResult<PagedEntity<OrderGetVModel>>.Fail("Lỗi máy chủ khi lấy danh sách đơn hàng"));
            }
        }

        // ============================================================
        // 📋 Lấy tất cả đơn hàng (Admin)
        // ============================================================
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                _logger.LogInformation("🔍 GetAllOrders - Page: {Page}, PageSize: {PageSize}", page, pageSize);

                var result = await _orderService.GetAllOrdersAsync(page, pageSize);

                if (!result.Success)
                {
                    _logger.LogWarning("⚠️ Failed to get all orders: {Message}", result.Message);
                    return BadRequest(result);
                }

                _logger.LogInformation("✅ Retrieved {Count} total orders", result.Data?.Items?.Count ?? 0);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error GetAllOrders");
                return StatusCode(500, ResponseResult<PagedEntity<OrderGetVModel>>.Fail("Lỗi máy chủ khi lấy danh sách đơn hàng"));
            }
        }

        // ============================================================
        // 📄 Lấy chi tiết 1 đơn hàng
        // ============================================================
        [HttpGet("{id:int}")]                    // <-- Generic route sau
        public async Task<IActionResult> GetOrderById(int id)
        {
            try
            {
                _logger.LogInformation("🔍 GetOrderById - OrderId: {OrderId}", id);

                var result = await _orderService.GetOrderByIdAsync(id);

                if (!result.Success)
                {
                    _logger.LogWarning("⚠️ Order not found: {OrderId}", id);
                    return NotFound(result);
                }

                _logger.LogInformation("✅ Retrieved order {OrderId} successfully", id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error GetOrderById {Id}", id);
                return StatusCode(500, ResponseResult<OrderGetVModel>.Fail("Lỗi máy chủ khi lấy chi tiết đơn hàng"));
            }
        }

        // ============================================================
        // 🟡 Cập nhật trạng thái đơn hàng (Admin)
        // ============================================================
        [HttpPut("status/{orderId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] OrderUpdateVModel model)
        {
            try
            {
                _logger.LogInformation("🔄 UpdateOrderStatus - OrderId: {OrderId}, NewStatus: {Status}", orderId, model.OrderStatus);

                var result = await _orderService.UpdateOrderStatusAsync(orderId, model.OrderStatus);

                if (!result.Success)
                {
                    _logger.LogWarning("⚠️ Failed to update order status: {Message}", result.Message);
                    return BadRequest(result);
                }

                _logger.LogInformation("✅ Updated order {OrderId} status to {Status}", orderId, model.OrderStatus);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error UpdateOrderStatus {OrderId}", orderId);
                return StatusCode(500, ResponseResult.Fail("Lỗi máy chủ khi cập nhật trạng thái"));
            }
        }

        // ============================================================
        // ❌ Hủy đơn hàng
        // ============================================================
        [HttpPut("cancel/{id:int}")]
        [Authorize]
        public async Task<IActionResult> CancelOrder(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(ResponseResult.Fail("Không thể xác thực người dùng"));
                }

                _logger.LogInformation("🔴 CancelOrder - OrderId: {OrderId}, UserId: {UserId}", id, userId);

                var result = await _orderService.CancelOrderAsync(id, userId);

                if (!result.Success)
                {
                    _logger.LogWarning("⚠️ Failed to cancel order: {Message}", result.Message);
                    return BadRequest(result);
                }

                _logger.LogInformation("✅ Canceled order {OrderId} successfully", id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error CancelOrder {Id}", id);
                return StatusCode(500, ResponseResult.Fail("Lỗi máy chủ khi hủy đơn hàng"));
            }
        }
    }
}
