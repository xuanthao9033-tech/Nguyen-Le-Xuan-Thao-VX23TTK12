using IphoneStoreBE.Common.Models;
using IphoneStoreBE.Context;
using IphoneStoreBE.Entities;
using IphoneStoreBE.Services.IServices;
using IphoneStoreBE.VModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace IphoneStoreBE.Services
{
    public class OrderService : IOrderService
    {
        private readonly IphoneStoreContext _context;
        private readonly ILogger<OrderService> _logger;

        public OrderService(IphoneStoreContext context, ILogger<OrderService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // 🧩 Helper: Lấy UserId từ HttpContext
        private int GetUserIdFromContext(HttpContext httpContext)
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                throw new UnauthorizedAccessException("User must be authenticated.");
            return userId;
        }

        // 🧾 Validate khi tạo đơn hàng
        public static string ValidateCreate(OrderCreateVModel model)
        {
            if (string.IsNullOrWhiteSpace(model.PaymentMethod))
                return "Phương thức thanh toán không hợp lệ.";

            model.PaymentMethod = model.PaymentMethod.Trim().ToUpperInvariant();
            var validPayments = new[] { "COD", "BANK" };
            if (!validPayments.Contains(model.PaymentMethod))
                return "Phương thức thanh toán không hợp lệ.";

            if (string.IsNullOrWhiteSpace(model.Recipient) ||
                string.IsNullOrWhiteSpace(model.PhoneNumber) ||
                string.IsNullOrWhiteSpace(model.AddressDetailRecipient))
                return "Vui lòng nhập đầy đủ thông tin giao hàng.";

            return string.Empty;
        }

        // 1️⃣ Tạo đơn hàng từ giỏ hàng
        public async Task<ResponseResult> ProcessPaymentAsync(OrderCreateVModel model, HttpContext httpContext)
        {
            try
            {
                var validationResult = ValidateCreate(model);
                if (!string.IsNullOrEmpty(validationResult))
                    return ResponseResult.Fail(validationResult);

                var userId = model.UserId > 0 ? model.UserId : GetUserIdFromContext(httpContext);

                var carts = await _context.Carts
                    .Include(c => c.Product)
                    .Where(c => c.UserId == userId && (c.IsActive == null || c.IsActive == true))
                    .ToListAsync();

                if (!carts.Any())
                    return ResponseResult.Fail("Giỏ hàng trống.");

                decimal total = carts.Sum(c => c.Quantity * (c.Product?.Price ?? 0));

                if (model.PaymentMethod == "BANK")
                {
                    model.ShippingPrice = 30000;
                    total += 30000;
                }
                else
                {
                    model.ShippingPrice = 0;
                }

                var orderAddress = new OrderAddress
                {
                    Recipient = model.Recipient,
                    PhoneNumber = model.PhoneNumber,
                    AddressDetailRecipient = model.AddressDetailRecipient,
                    City = model.City,
                    District = model.District,
                    Ward = model.Ward,
                    UserId = userId,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                _context.OrderAddresses.Add(orderAddress);
                await _context.SaveChangesAsync();

                var orderCode = $"ORD{DateTime.UtcNow:yyyyMMddHHmmss}{Guid.NewGuid().ToString()[..4]}";
                var order = new Order
                {
                    UserId = userId,
                    OrderCode = orderCode,
                    OrderDate = DateTime.UtcNow,
                    Total = total,
                    ShippingPrice = model.ShippingPrice,
                    PaymentMethod = model.PaymentMethod,
                    OrderStatus = "Chờ xác nhận",
                    OrderAddId = orderAddress.Id,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = $"User_{userId}"
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                foreach (var cart in carts)
                {
                    if (cart.Product == null) continue;

                    var orderDetail = new OrderDetail
                    {
                        Quantity = cart.Quantity,
                        Price = cart.Product.Price,
                        ProductId = cart.ProductId,
                        OrderId = order.Id,
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = $"User_{userId}"
                    };
                    _context.OrderDetails.Add(orderDetail);

                    cart.IsActive = false;
                    cart.UpdatedDate = DateTime.UtcNow;
                    cart.UpdatedBy = $"User_{userId}";
                }

                await _context.SaveChangesAsync();
                return ResponseResult.Ok("Đặt hàng thành công! Đơn hàng đang chờ xác nhận.");
            }
            catch (UnauthorizedAccessException)
            {
                return ResponseResult.Fail("Bạn phải đăng nhập để tạo đơn hàng.");
            }
            catch (Exception ex)
            {
                return ResponseResult.Fail($"Lỗi khi tạo đơn hàng: {ex.Message}");
            }
        }

        // 2️⃣ Lấy đơn hàng theo ID
        public async Task<ResponseResult<OrderGetVModel>> GetOrderByIdAsync(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderDetails).ThenInclude(od => od.Product).ThenInclude(p => p.ProductImages)
                    .Include(o => o.OrderAdd)
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.Id == id && o.IsActive == true);

                if (order == null)
                    return ResponseResult<OrderGetVModel>.Fail("Không tìm thấy đơn hàng.");

                var orderVModel = new OrderGetVModel
                {
                    Id = order.Id,
                    OrderCode = order.OrderCode,
                    OrderDate = order.OrderDate,
                    Total = order.Total ?? 0,
                    ShippingPrice = order.ShippingPrice,
                    PaymentMethod = order.PaymentMethod,
                    OrderStatus = order.OrderStatus,
                    UserName = order.User?.UserName ?? "N/A",
                    OrderAddress = order.OrderAdd != null ? new OrderAddressGetVModel
                    {
                        Recipient = order.OrderAdd.Recipient,
                        PhoneNumber = order.OrderAdd.PhoneNumber,
                        AddressDetailRecipient = order.OrderAdd.AddressDetailRecipient,
                        City = order.OrderAdd.City ?? string.Empty,
                        District = order.OrderAdd.District ?? string.Empty,
                        Ward = order.OrderAdd.Ward ?? string.Empty
                    } : null,
                    OrderDetails = order.OrderDetails?.Select(od => new OrderDetailGetVModel
                    {
                        ProductId = od.ProductId ?? 0,
                        ProductName = od.Product?.ProductName ?? "N/A",
                        ProductImage = od.Product?.ImageUrl,
                        Quantity = od.Quantity,
                        Price = od.Price
                    }).ToList() ?? new List<OrderDetailGetVModel>()
                };

                return ResponseResult<OrderGetVModel>.Ok(orderVModel, "Lấy đơn hàng thành công!");
            }
            catch (Exception ex)
            {
                return ResponseResult<OrderGetVModel>.Fail($"Lỗi: {ex.Message}");
            }
        }

        // 3️⃣ Lấy danh sách đơn hàng của user (phân trang) - CẢI TIẾN LOGGING
        public async Task<ResponseResult<PagedEntity<OrderGetVModel>>> GetOrdersByUserIdAsync(int userId, int page = 1, int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("🔍 [GetOrdersByUserIdAsync] START - UserId: {UserId}, Page: {Page}, PageSize: {PageSize}", 
                    userId, page, pageSize);

                // ✅ Bước 1: Query orders từ database
                _logger.LogDebug("📊 [GetOrdersByUserIdAsync] Querying Orders table...");
                
                var orders = await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderAdd)
                    .Include(o => o.OrderDetails).ThenInclude(od => od.Product).ThenInclude(p => p.ProductImages)
                    .Where(o => o.UserId == userId && o.IsActive == true)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                _logger.LogInformation("✅ [GetOrdersByUserIdAsync] Query completed - Found {Count} orders for UserId: {UserId}", 
                    orders.Count, userId);

                // ✅ Bước 2: Kiểm tra nếu không có đơn hàng
                if (!orders.Any())
                {
                    _logger.LogWarning("⚠️ [GetOrdersByUserIdAsync] NO ORDERS FOUND for UserId: {UserId}. Reasons to check:", userId);
                    _logger.LogWarning("   - User có đơn hàng trong DB không? Check: SELECT * FROM Orders WHERE UserId = {UserId}", userId);
                    _logger.LogWarning("   - IsActive = true không? Check: SELECT * FROM Orders WHERE UserId = {UserId} AND IsActive = 1", userId);
                    _logger.LogWarning("   - User đã đặt hàng bao giờ chưa?", userId);
                    
                    var pagedEmpty = new PagedEntity<OrderGetVModel>(new List<OrderGetVModel>(), page, pageSize);
                    return ResponseResult<PagedEntity<OrderGetVModel>>.Ok(pagedEmpty, 
                        $"User {userId} chưa có đơn hàng nào.");
                }

                // ✅ Bước 3: Transform sang OrderGetVModel
                _logger.LogDebug("🔄 [GetOrdersByUserIdAsync] Transforming {Count} orders to ViewModels...", orders.Count);
                
                var orderVModels = new List<OrderGetVModel>();
                
                foreach (var o in orders)
                {
                    try
                    {
                        // Log chi tiết từng order
                        _logger.LogDebug("   📦 Processing Order: Id={OrderId}, Code={OrderCode}, Status={Status}", 
                            o.Id, o.OrderCode, o.OrderStatus);

                        var orderVModel = new OrderGetVModel
                        {
                            Id = o.Id,
                            OrderCode = o.OrderCode,
                            OrderDate = o.OrderDate,
                            Total = o.Total ?? 0,
                            ShippingPrice = o.ShippingPrice,
                            PaymentMethod = o.PaymentMethod,
                            OrderStatus = o.OrderStatus,
                            UserName = o.User?.UserName ?? "N/A",
                            OrderAddress = o.OrderAdd != null ? new OrderAddressGetVModel
                            {
                                Recipient = o.OrderAdd.Recipient,
                                PhoneNumber = o.OrderAdd.PhoneNumber,
                                AddressDetailRecipient = o.OrderAdd.AddressDetailRecipient,
                                City = o.OrderAdd.City ?? string.Empty,
                                District = o.OrderAdd.District ?? string.Empty,
                                Ward = o.OrderAdd.Ward ?? string.Empty
                            } : null,
                            OrderDetails = o.OrderDetails?.Select(od => new OrderDetailGetVModel
                            {
                                ProductId = od.ProductId ?? 0,
                                ProductName = od.Product?.ProductName ?? "N/A",
                                ProductImage = od.Product?.ImageUrl,
                                Quantity = od.Quantity,
                                Price = od.Price
                            }).ToList() ?? new List<OrderDetailGetVModel>()
                        };

                        // ✅ Log thông tin OrderDetails
                        if (orderVModel.OrderDetails.Any())
                        {
                            _logger.LogDebug("      ✅ Order {OrderId} has {Count} order details", 
                                o.Id, orderVModel.OrderDetails.Count);
                        }
                        else
                        {
                            _logger.LogWarning("      ⚠️ Order {OrderId} has NO order details! Check OrderDetails table.", o.Id);
                        }

                        // ✅ Log thông tin Address
                        if (orderVModel.OrderAddress == null)
                        {
                            _logger.LogWarning("      ⚠️ Order {OrderId} has NO address! OrderAddId = {AddressId}", 
                                o.Id, o.OrderAddId);
                        }

                        orderVModels.Add(orderVModel);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ [GetOrdersByUserIdAsync] Error processing Order {OrderId}: {Message}", 
                            o.Id, ex.Message);
                        // Continue processing other orders
                    }
                }

                _logger.LogInformation("✅ [GetOrdersByUserIdAsync] Successfully transformed {Count} orders", orderVModels.Count);

                // ✅ Bước 4: Tạo PagedEntity
                _logger.LogDebug("📄 [GetOrdersByUserIdAsync] Creating paged result...");
                
                var pagedOrders = new PagedEntity<OrderGetVModel>(orderVModels, page, pageSize);
                
                _logger.LogInformation("✅ [GetOrdersByUserIdAsync] SUCCESS - Returning {ItemCount}/{TotalItems} orders (Page {Page}/{TotalPages})", 
                    pagedOrders.Items.Count, pagedOrders.TotalItems, pagedOrders.PageIndex, pagedOrders.TotalPages);

                return ResponseResult<PagedEntity<OrderGetVModel>>.Ok(pagedOrders, 
                    $"Lấy danh sách {pagedOrders.Items.Count} đơn hàng thành công!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [GetOrdersByUserIdAsync] EXCEPTION for UserId {UserId}: {Message}\nStackTrace: {StackTrace}", 
                    userId, ex.Message, ex.StackTrace);
                
                return ResponseResult<PagedEntity<OrderGetVModel>>.Fail(
                    $"Lỗi khi lấy danh sách đơn hàng: {ex.Message}");
            }
        }

        // 4️⃣ Lấy tất cả đơn hàng (Admin)
        public async Task<ResponseResult<PagedEntity<OrderGetVModel>>> GetAllOrdersAsync(int page = 1, int pageSize = 20)
        {
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderAdd)
                    .Include(o => o.OrderDetails).ThenInclude(od => od.Product).ThenInclude(p => p.ProductImages)
                    .Where(o => o.IsActive == true)
                    .OrderByDescending(o => o.CreatedDate)
                    .ToListAsync();

                var orderVModels = orders.Select(o => new OrderGetVModel
                {
                    Id = o.Id,
                    OrderCode = o.OrderCode,
                    OrderDate = o.OrderDate,
                    Total = o.Total ?? 0,
                    ShippingPrice = o.ShippingPrice,
                    PaymentMethod = o.PaymentMethod,
                    OrderStatus = o.OrderStatus,
                    UserName = o.User?.UserName ?? "N/A",
                    OrderAddress = o.OrderAdd != null ? new OrderAddressGetVModel
                    {
                        Recipient = o.OrderAdd.Recipient,
                        PhoneNumber = o.OrderAdd.PhoneNumber,
                        AddressDetailRecipient = o.OrderAdd.AddressDetailRecipient,
                        City = o.OrderAdd.City ?? string.Empty,
                        District = o.OrderAdd.District ?? string.Empty,
                        Ward = o.OrderAdd.Ward ?? string.Empty
                    } : null,
                    OrderDetails = o.OrderDetails?.Select(od => new OrderDetailGetVModel
                    {
                        ProductId = od.ProductId ?? 0,
                        ProductName = od.Product?.ProductName ?? "N/A",
                        ProductImage = od.Product?.ImageUrl,
                        Quantity = od.Quantity,
                        Price = od.Price
                    }).ToList() ?? new List<OrderDetailGetVModel>()
                }).ToList();

                var pagedOrders = new PagedEntity<OrderGetVModel>(orderVModels, page, pageSize);
                return ResponseResult<PagedEntity<OrderGetVModel>>.Ok(pagedOrders, "Lấy danh sách đơn hàng thành công!");
            }
            catch (Exception ex)
            {
                return ResponseResult<PagedEntity<OrderGetVModel>>.Fail($"Lỗi khi lấy danh sách đơn hàng: {ex.Message}");
            }
        }

        // 5️⃣ Cập nhật trạng thái đơn hàng (Admin)
        public async Task<ResponseResult> UpdateOrderStatusAsync(int orderId, string status)
        {
            try
            {
                var validStatuses = new[]
                {
                    "Chờ xác nhận",
                    "Đã xác nhận",
                    "Đang chuẩn bị",
                    "Đang giao hàng",
                    "Đã giao thành công",
                    "Đã hủy"
                };

                if (!validStatuses.Contains(status))
                    return ResponseResult.Fail("Trạng thái đơn hàng không hợp lệ.");

                var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.IsActive == true);
                if (order == null)
                    return ResponseResult.Fail("Không tìm thấy đơn hàng.");

                order.OrderStatus = status;
                order.UpdatedDate = DateTime.UtcNow;
                order.UpdatedBy = "Admin";
                await _context.SaveChangesAsync();

                return ResponseResult.Ok($"Đã cập nhật trạng thái đơn hàng thành: {status}");
            }
            catch (Exception ex)
            {
                return ResponseResult.Fail($"Lỗi khi cập nhật: {ex.Message}");
            }
        }

        // 6️⃣ Hủy đơn hàng
        public async Task<ResponseResult> CancelOrderAsync(int orderId, int userId)
        {
            try
            {
                var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId && o.IsActive == true);
                if (order == null)
                    return ResponseResult.Fail("Không tìm thấy đơn hàng.");

                if (order.UserId != userId)
                    return ResponseResult.Fail("Bạn không có quyền hủy đơn hàng này.");

                if (order.OrderStatus == "Đã giao thành công")
                    return ResponseResult.Fail("Không thể hủy đơn hàng đã giao.");

                order.OrderStatus = "Đã hủy";
                order.UpdatedDate = DateTime.UtcNow;
                order.UpdatedBy = $"User_{userId}";
                await _context.SaveChangesAsync();

                return ResponseResult.Ok("Đã hủy đơn hàng thành công.");
            }
            catch (Exception ex)
            {
                return ResponseResult.Fail($"Lỗi khi hủy đơn hàng: {ex.Message}");
            }
        }
    }
}