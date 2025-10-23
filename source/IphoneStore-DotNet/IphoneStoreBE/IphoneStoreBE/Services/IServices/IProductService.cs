using IphoneStoreBE.Common.Models;
using IphoneStoreBE.VModels;

namespace IphoneStoreBE.Services.IServices
{
    public interface IProductService
    {
        // Lấy danh sách tất cả sản phẩm
        Task<ResponseResult<List<ProductGetVModel>>> GetAllAsync();

        // Lấy sản phẩm theo ID
        Task<ResponseResult<ProductGetVModel?>> GetByIdAsync(int id);

        // Tạo mới sản phẩm
        Task<ResponseResult> CreateAsync(ProductCreateVModel model);

        // Xóa sản phẩm
        Task<ResponseResult> DeleteAsync(int id);

        // Cập nhật sản phẩm
        Task<ResponseResult> UpdateAsync(ProductUpdateVModel model);
    }
}
