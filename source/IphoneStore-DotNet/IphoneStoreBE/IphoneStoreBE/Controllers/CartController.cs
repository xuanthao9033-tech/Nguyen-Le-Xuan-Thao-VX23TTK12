using IphoneStoreBE.Common.Models;
using IphoneStoreBE.Context;
using IphoneStoreBE.Entities;
using IphoneStoreBE.VModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IphoneStoreBE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly IphoneStoreContext _context;

        public CartController(IphoneStoreContext context)
        {
            _context = context;
        }

        // ================================================================
        // 🛒 Lấy danh sách sản phẩm trong giỏ hàng
        // ================================================================
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCart(int userId)
        {
            try
            {
                var cartItems = await _context.Carts
                    .Include(c => c.Product)
                    .Where(c => c.UserId == userId && (c.IsActive == null || c.IsActive == true))
                    .Select(c => new CartGetVModel
                    {
                        Id = c.Id,
                        ProductId = c.ProductId,
                        ProductName = c.Product.ProductName,
                        ImageUrl = !string.IsNullOrEmpty(c.Product.ImageUrl)
                            ? c.Product.ImageUrl.StartsWith("http")
                                ? c.Product.ImageUrl
                                : $"/images/products/{Path.GetFileName(c.Product.ImageUrl)}"
                            : "/images/default-product.jpg",
                        Quantity = c.Quantity,
                        Price = c.Product.Price,
                        IsActive = c.IsActive,
                        CreatedDate = c.CreatedDate,
                        UpdatedDate = c.UpdatedDate,
                        CreatedBy = c.CreatedBy,
                        UpdatedBy = c.UpdatedBy,
                        UserId = c.UserId
                    })
                    .ToListAsync();

                return Ok(ResponseResult.Ok(cartItems, "Lấy giỏ hàng thành công"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseResult.Fail($"Lỗi khi lấy giỏ hàng: {ex.InnerException?.Message ?? ex.Message}"));
            }
        }

        // ================================================================
        // ➕ Thêm sản phẩm vào giỏ hàng (FIXED)
        // ================================================================
        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest model)
        {
            try
            {
                if (model == null || model.UserId <= 0 || model.ProductId <= 0)
                    return BadRequest(ResponseResult.Fail("Dữ liệu không hợp lệ."));

                // 🔹 Kiểm tra xem sản phẩm đã tồn tại trong giỏ (kể cả Inactive)
                var existing = await _context.Carts
                    .FirstOrDefaultAsync(c => c.UserId == model.UserId && c.ProductId == model.ProductId);

                if (existing != null)
                {
                    // 🔹 Nếu tồn tại, cập nhật lại (kích hoạt nếu bị disable)
                    existing.IsActive = true;
                    existing.Quantity += model.Quantity;
                    existing.UpdatedDate = DateTime.UtcNow;
                    existing.UpdatedBy = "User";
                }
                else
                {
                    // 🔹 Nếu chưa có thì thêm mới
                    var newCart = new Cart
                    {
                        UserId = model.UserId,
                        ProductId = model.ProductId,
                        Quantity = model.Quantity,
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = "User"
                    };
                    _context.Carts.Add(newCart);
                }

                await _context.SaveChangesAsync();
                return Ok(ResponseResult.Ok("✅ Sản phẩm đã được thêm vào giỏ hàng."));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ [AddToCart Error] {ex.InnerException?.Message ?? ex.Message}");
                return StatusCode(500, ResponseResult.Fail($"Lỗi khi thêm giỏ hàng: {ex.InnerException?.Message ?? ex.Message}"));
            }
        }

        // ================================================================
        // ✏️ Cập nhật số lượng
        // ================================================================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuantity(int id, [FromBody] int quantity)
        {
            try
            {
                var cart = await _context.Carts.FindAsync(id);
                if (cart == null)
                    return NotFound(ResponseResult.Fail("Không tìm thấy sản phẩm trong giỏ hàng."));

                if (quantity <= 0)
                    return BadRequest(ResponseResult.Fail("Số lượng phải lớn hơn 0."));

                cart.Quantity = quantity;
                cart.UpdatedDate = DateTime.UtcNow;
                cart.UpdatedBy = "User";

                await _context.SaveChangesAsync();
                return Ok(ResponseResult.Ok("Cập nhật số lượng thành công."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseResult.Fail($"Lỗi cập nhật: {ex.InnerException?.Message ?? ex.Message}"));
            }
        }

        // ================================================================
        // ❌ Xóa sản phẩm trong giỏ hàng
        // ================================================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            try
            {
                var cart = await _context.Carts.FindAsync(id);
                if (cart == null)
                    return NotFound(ResponseResult.Fail("Không tìm thấy sản phẩm trong giỏ hàng."));

                _context.Carts.Remove(cart);
                await _context.SaveChangesAsync();
                return Ok(ResponseResult.Ok("Đã xóa sản phẩm khỏi giỏ hàng."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseResult.Fail($"Lỗi khi xóa sản phẩm: {ex.InnerException?.Message ?? ex.Message}"));
            }
        }

        // ================================================================
        // 🧹 Xóa toàn bộ giỏ hàng
        // ================================================================
        [HttpDelete("Clear/{userId}")]
        public async Task<IActionResult> ClearCart(int userId)
        {
            try
            {
                var items = _context.Carts.Where(c => c.UserId == userId);
                if (!items.Any())
                    return Ok(ResponseResult.Ok("Giỏ hàng đã trống."));

                _context.Carts.RemoveRange(items);
                await _context.SaveChangesAsync();

                return Ok(ResponseResult.Ok("Đã xóa toàn bộ giỏ hàng."));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ResponseResult.Fail($"Lỗi khi xóa giỏ hàng: {ex.InnerException?.Message ?? ex.Message}"));
            }
        }
    }

    // 🔸 Model yêu cầu cho API thêm giỏ hàng
    public class AddToCartRequest
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
