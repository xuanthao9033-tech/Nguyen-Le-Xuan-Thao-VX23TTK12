using IphoneStoreBE.Common.Models;
using IphoneStoreBE.VModels;

namespace IphoneStoreBE.Services.IServices
{
    public interface IProductImageService
    {
        // Lấy danh sách tất cả ảnh sản phẩm (có thể filter theo ProductId)
        Task<ResponseResult<List<ProductImageGetVModel>>> GetAllAsync(int? productId = null);
        // Lấy ảnh theo ID
        Task<ResponseResult<ProductImageGetVModel?>> GetByIdAsync(int id);
        // Tạo mới ảnh sản phẩm (upload file)
        Task<ResponseResult> CreateAsync(ProductImageCreateVModel model, IFormFile imageFile);
        // Xóa ảnh
        Task<ResponseResult> DeleteAsync(int id);
        // Cập nhật ảnh (metadata như AltText, IsActive)
        Task<ResponseResult> UpdateAsync(ProductImageUpdateVModel model);
    }
}