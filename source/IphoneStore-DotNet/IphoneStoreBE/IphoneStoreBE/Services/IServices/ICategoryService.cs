using IphoneStoreBE.VModels;
using IphoneStoreBE.Common.Models;

namespace IphoneStoreBE.Services.IServices
{
    public interface ICategoryService
    {
        // Lấy danh sách tất cả danh mục
        Task<ResponseResult<List<CategoryGetVModel>>> GetAllAsync();
        // Lấy danh mục theo ID
        Task<ResponseResult<CategoryGetVModel?>> GetByIdAsync(int id);
        // Tạo mới danh mục
        Task<ResponseResult> CreateAsync(CategoryCreateVModel model);
        // Xóa danh mục
        Task<ResponseResult> DeleteAsync(int id);
        // Cập nhật danh mục
        Task<ResponseResult> UpdateAsync(CategoryUpdateVModel model);
    }
}
