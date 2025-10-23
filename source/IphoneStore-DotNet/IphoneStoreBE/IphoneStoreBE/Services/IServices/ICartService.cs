using IphoneStoreBE.Common.Models;
using IphoneStoreBE.VModels;
using Microsoft.AspNetCore.Http;

namespace IphoneStoreBE.Services.IServices
{
    public interface ICartService
    {
        Task<ResponseResult<List<CartGetVModel>>> GetAllAsync(HttpContext httpContext);
        Task<ResponseResult<CartGetVModel?>> GetByIdAsync(int id, HttpContext httpContext);
        Task<ResponseResult> CreateAsync(CartCreateVModel model, HttpContext httpContext);
        Task<ResponseResult> UpdateAsync(CartUpdateVModel model, HttpContext httpContext);
        Task<ResponseResult> DeleteAsync(int id, HttpContext httpContext);
        Task<ResponseResult> ClearAllAsync(int userId);
    }
}
