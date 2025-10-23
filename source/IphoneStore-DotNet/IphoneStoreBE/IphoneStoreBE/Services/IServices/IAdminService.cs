using IphoneStoreBE.Common.Models;
using IphoneStoreBE.VModels;

namespace IphoneStoreBE.Services.IServices
{
    public interface IAdminService
    {
        Task<ResponseResult<AdminStatisticsVModel>> GetStatisticsAsync();
        Task<ResponseResult<List<OrderGetVModel>>> GetRecentOrdersAsync(int count = 10);
    }
}