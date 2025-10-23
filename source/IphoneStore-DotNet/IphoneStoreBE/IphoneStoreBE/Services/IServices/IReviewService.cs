using IphoneStoreBE.Common.Models;
using IphoneStoreBE.VModels;

namespace IphoneStoreBE.Services.IServices
{
    public interface IReviewService
    {
        // Lấy danh sách tất cả Review theo ProductId (công khai, chỉ active)
        Task<ResponseResult<List<ReviewGetVModel>>> GetAllAsync(int? productId = null);
        // Lấy Review theo ID (công khai)
        Task<ResponseResult<ReviewGetVModel?>> GetByIdAsync(int id);
        // Tạo mới Review (yêu cầu auth)
        Task<ResponseResult> CreateAsync(ReviewCreateVModel model, HttpContext httpContext);
        // Xóa Review theo ID (yêu cầu auth, của user hiện tại)
        Task<ResponseResult> DeleteAsync(int id, HttpContext httpContext);
        // Cập nhật Review (yêu cầu auth, của user hiện tại)
        Task<ResponseResult> UpdateAsync(ReviewUpdateVModel model, HttpContext httpContext);
    }
}