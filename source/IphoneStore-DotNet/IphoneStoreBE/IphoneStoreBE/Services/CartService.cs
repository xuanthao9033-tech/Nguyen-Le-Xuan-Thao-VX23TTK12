using IphoneStoreBE.Common.Models;
using IphoneStoreBE.Context;
using IphoneStoreBE.Entities;
using IphoneStoreBE.Services.IServices;
using IphoneStoreBE.VModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IphoneStoreBE.Services
{
    public class CartService : ICartService
    {
        private readonly IphoneStoreContext _context;

        public CartService(IphoneStoreContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // ✅ Lấy UserId từ token/session
        private int? TryGetUserId(HttpContext httpContext)
        {
            var claim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(claim, out int id)) return id;
            return null;
        }

        // ================================================================
        // 🛒 1️⃣ Lấy tất cả sản phẩm trong giỏ hàng
        // ================================================================
        public async Task<ResponseResult<List<CartGetVModel>>> GetAllAsync(HttpContext httpContext)
        {
            try
            {
                var userId = TryGetUserId(httpContext);
                if (userId == null) return ResponseResult<List<CartGetVModel>>.Fail("Chưa đăng nhập.");

                var carts = await _context.Carts
                    .Include(c => c.Product)
                    .Where(c => c.UserId == userId.Value && (c.IsActive == null || c.IsActive == true))
                    .OrderByDescending(c => c.UpdatedDate)
                    .Select(c => new CartGetVModel
                    {
                        Id = c.Id,
                        UserId = c.UserId,
                        ProductId = c.ProductId,
                        ProductName = c.Product.ProductName,
                        Price = c.Product.Price,
                        ImageUrl = c.Product.ImageUrl,
                        Quantity = c.Quantity,
                        CreatedDate = c.CreatedDate,
                        UpdatedDate = c.UpdatedDate
                    })
                    .ToListAsync();

                return ResponseResult<List<CartGetVModel>>.Ok(carts, "Lấy giỏ hàng thành công");
            }
            catch (Exception ex)
            {
                return ResponseResult<List<CartGetVModel>>.Fail($"Lỗi: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        // ================================================================
        // 🔍 2️⃣ Lấy chi tiết 1 item giỏ hàng
        // ================================================================
        public async Task<ResponseResult<CartGetVModel?>> GetByIdAsync(int id, HttpContext httpContext)
        {
            try
            {
                var cart = await _context.Carts
                    .Include(c => c.Product)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (cart == null)
                    return ResponseResult<CartGetVModel?>.Fail("Không tìm thấy sản phẩm trong giỏ hàng.");

                var result = new CartGetVModel
                {
                    Id = cart.Id,
                    UserId = cart.UserId,
                    ProductId = cart.ProductId,
                    ProductName = cart.Product?.ProductName,
                    Price = cart.Product?.Price,
                    ImageUrl = cart.Product?.ImageUrl,
                    Quantity = cart.Quantity,
                    CreatedDate = cart.CreatedDate,
                    UpdatedDate = cart.UpdatedDate
                };

                return ResponseResult<CartGetVModel?>.Ok(result, "Lấy chi tiết giỏ hàng thành công");
            }
            catch (Exception ex)
            {
                return ResponseResult<CartGetVModel?>.Fail($"Lỗi khi lấy chi tiết: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        // ================================================================
        // ➕ 3️⃣ Thêm sản phẩm vào giỏ hàng
        // ================================================================
        public async Task<ResponseResult> CreateAsync(CartCreateVModel model, HttpContext httpContext)
        {
            try
            {
                var userId = TryGetUserId(httpContext) ?? model.UserId;
                if (userId == null || userId == 0)
                    return ResponseResult.Fail("Không xác định người dùng.");

                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == model.ProductId);
                if (product == null)
                    return ResponseResult.Fail("Sản phẩm không tồn tại.");

                var existing = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == model.ProductId);

                if (existing != null)
                {
                    existing.Quantity += model.Quantity;
                    existing.IsActive = true;
                    existing.UpdatedDate = DateTime.UtcNow;
                    existing.UpdatedBy = $"User_{userId}";
                }
                else
                {
                    var newCart = new Cart
                    {
                        UserId = userId.Value,
                        ProductId = model.ProductId,
                        Quantity = model.Quantity,
                        CreatedBy = $"User_{userId}",
                        CreatedDate = DateTime.UtcNow,
                        IsActive = true
                    };
                    _context.Carts.Add(newCart);
                }

                await _context.SaveChangesAsync();
                return ResponseResult.Ok("Đã thêm sản phẩm vào giỏ hàng.");
            }
            catch (Exception ex)
            {
                return ResponseResult.Fail($"Lỗi khi thêm giỏ hàng: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        // ================================================================
        // ✏️ 4️⃣ Cập nhật giỏ hàng
        // ================================================================
        public async Task<ResponseResult> UpdateAsync(CartUpdateVModel model, HttpContext httpContext)
        {
            try
            {
                var cart = await _context.Carts.FindAsync(model.Id);
                if (cart == null)
                    return ResponseResult.Fail("Không tìm thấy sản phẩm trong giỏ hàng.");

                if (model.Quantity > 0)
                    cart.Quantity = model.Quantity;

                if (model.IsActive != null)
                    cart.IsActive = model.IsActive;

                cart.UpdatedDate = DateTime.UtcNow;
                cart.UpdatedBy = "User";

                await _context.SaveChangesAsync();
                return ResponseResult.Ok("Cập nhật giỏ hàng thành công.");
            }
            catch (Exception ex)
            {
                return ResponseResult.Fail($"Lỗi khi cập nhật: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        // ================================================================
        // ❌ 5️⃣ Xóa sản phẩm trong giỏ hàng
        // ================================================================
        public async Task<ResponseResult> DeleteAsync(int id, HttpContext httpContext)
        {
            try
            {
                var cart = await _context.Carts.FindAsync(id);
                if (cart == null)
                    return ResponseResult.Fail("Không tìm thấy sản phẩm trong giỏ hàng.");

                _context.Carts.Remove(cart);
                await _context.SaveChangesAsync();
                return ResponseResult.Ok("Đã xóa sản phẩm khỏi giỏ hàng.");
            }
            catch (Exception ex)
            {
                return ResponseResult.Fail($"Lỗi khi xóa: {ex.InnerException?.Message ?? ex.Message}");
            }
        }
        // Thêm cuối file CartService.cs
        // ================================================================
        // 🧹 6️⃣ Xóa toàn bộ giỏ hàng của user
        // ================================================================
        public async Task<ResponseResult> ClearAllAsync(int userId)
        {
            try
            {
                var carts = await _context.Carts.Where(c => c.UserId == userId).ToListAsync();
                if (!carts.Any())
                    return ResponseResult.Fail("Giỏ hàng trống.");

                _context.Carts.RemoveRange(carts);
                await _context.SaveChangesAsync();
                return ResponseResult.Ok("Đã xóa toàn bộ giỏ hàng.");
            }
            catch (Exception ex)
            {
                return ResponseResult.Fail($"Lỗi khi xóa giỏ hàng: {ex.Message}");
            }
        }

    }
}
