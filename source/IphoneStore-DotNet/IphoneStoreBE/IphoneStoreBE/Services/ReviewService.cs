using IphoneStoreBE.Common.Models;
using IphoneStoreBE.Context;
using IphoneStoreBE.Entities;
using IphoneStoreBE.Mappings;
using IphoneStoreBE.Services.IServices;
using IphoneStoreBE.VModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace IphoneStoreBE.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IphoneStoreContext _context;

        public ReviewService(IphoneStoreContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // Helper: Lấy UserId từ HttpContext (ClaimsPrincipal) - chỉ dùng cho method cần auth
        private int GetUserIdFromContext(HttpContext httpContext)
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("User must be authenticated.");
            }
            return userId;
        }

        // ----- [Validation phương thức tạo mới Review] ------------------------- 
        public static string ValidateCreate(ReviewCreateVModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Comment))
                return "Comment là bắt buộc.";
            if (model.Comment.Length > 500)
                return "Comment phải ít hơn 500 ký tự.";
            if (model.ProductId <= 0)
                return "ProductId phải lớn hơn 0.";
            return string.Empty;
        }

        // ----- [Validation phương thức cập nhật Review] ------------------------- 
        public static string ValidateUpdate(ReviewUpdateVModel model)
        {
            if (model.Id <= 0)
                return "Id phải lớn hơn 0.";
            if (!string.IsNullOrWhiteSpace(model.Comment) && model.Comment.Length > 500)
                return "Comment phải ít hơn 500 ký tự.";
            if (model.IsActive == null)
                return "IsActive phải là true hoặc false.";
            return string.Empty;
        }

        // [01.] --- Phương thức tạo mới Review
        public async Task<ResponseResult> CreateAsync(ReviewCreateVModel model, HttpContext httpContext)
        {
            try
            {
                var validationResult = ValidateCreate(model);
                if (!string.IsNullOrEmpty(validationResult))
                    return ResponseResult.Fail(validationResult);

                var userId = GetUserIdFromContext(httpContext);

                // Kiểm tra Product tồn tại
                var productExists = await _context.Products.AnyAsync(p => p.Id == model.ProductId);
                if (!productExists)
                    return ResponseResult.Fail($"Sản phẩm với Id: {model.ProductId} không tồn tại.");

                // Kiểm tra duplicate review (cùng UserId và ProductId)
                var existingReview = await _context.Reviews
                    .AnyAsync(r => r.ProductId == model.ProductId && r.UserId == userId && r.IsActive == true);
                if (existingReview)
                    return ResponseResult.Fail("Bạn đã đánh giá sản phẩm này rồi.");

                // Tạo mới
                var review = model.ToEntity(userId);
                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                return ResponseResult.Ok("Đã tạo đánh giá thành công!");
            }
            catch (UnauthorizedAccessException)
            {
                return ResponseResult.Fail("Bạn phải đăng nhập để tạo đánh giá.");
            }
            catch (Exception ex)
            {
                return ResponseResult.Fail($"Đã có lỗi phát sinh khi tạo đánh giá: {ex.Message}");
            }
        }

        // [02.] --- Phương thức xóa Review theo ID
        public async Task<ResponseResult> DeleteAsync(int id, HttpContext httpContext)
        {
            try
            {
                var userId = GetUserIdFromContext(httpContext);

                var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
                if (review == null)
                    return ResponseResult.Fail($"Không tìm thấy đánh giá với Id: {id}.");

                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();

                return ResponseResult.Ok($"Đã xóa đánh giá với Id: {id} thành công.");
            }
            catch (UnauthorizedAccessException)
            {
                return ResponseResult.Fail("Bạn phải đăng nhập để xóa đánh giá.");
            }
            catch (Exception ex)
            {
                return ResponseResult.Fail($"Có lỗi phát sinh: {ex.Message}");
            }
        }

        // [03.] --- Phương thức lấy tất cả Review (filter theo ProductId, chỉ active)
        public async Task<ResponseResult<List<ReviewGetVModel>>> GetAllAsync(int? productId = null)
        {
            try
            {
                var query = _context.Reviews.Where(r => r.IsActive == true).AsQueryable();
                if (productId.HasValue)
                    query = query.Where(r => r.ProductId == productId);

                var reviews = await query
                    .OrderByDescending(r => r.CreatedDate) // Mới nhất trước
                    .Select(r => r.ToGetVModel())
                    .ToListAsync();
                return ResponseResult<List<ReviewGetVModel>>.Ok(reviews, "Lấy danh sách đánh giá thành công!");
            }
            catch (Exception ex)
            {
                return ResponseResult<List<ReviewGetVModel>>.Fail($"Có lỗi phát sinh: {ex.Message}");
            }
        }

        // [04.] --- Phương thức lấy Review theo ID
        public async Task<ResponseResult<ReviewGetVModel?>> GetByIdAsync(int id)
        {
            try
            {
                var review = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.Id == id && r.IsActive == true);

                if (review == null)
                    return ResponseResult<ReviewGetVModel?>.Fail($"Không tìm thấy đánh giá với Id: {id}.");

                var reviewVModel = review.ToGetVModel();
                return ResponseResult<ReviewGetVModel?>.Ok(reviewVModel, "Lấy đánh giá theo Id thành công!");
            }
            catch (Exception ex)
            {
                return ResponseResult<ReviewGetVModel?>.Fail($"Có lỗi phát sinh: {ex.Message}");
            }
        }

        // [05.] --- Phương thức cập nhật Review
        public async Task<ResponseResult> UpdateAsync(ReviewUpdateVModel model, HttpContext httpContext)
        {
            try
            {
                var validationResult = ValidateUpdate(model);
                if (!string.IsNullOrEmpty(validationResult))
                    return ResponseResult.Fail(validationResult);

                var userId = GetUserIdFromContext(httpContext);

                var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == model.Id && r.UserId == userId);
                if (review == null)
                    return ResponseResult.Fail($"Không tìm thấy đánh giá với Id: {model.Id}.");

                review.UpdateEntity(model);
                await _context.SaveChangesAsync();

                return ResponseResult.Ok("Đã cập nhật đánh giá thành công!");
            }
            catch (UnauthorizedAccessException)
            {
                return ResponseResult.Fail("Bạn phải đăng nhập để cập nhật đánh giá.");
            }
            catch (Exception ex)
            {
                return ResponseResult.Fail($"Đã có lỗi phát sinh khi cập nhật đánh giá: {ex.Message}");
            }
        }
    }
}