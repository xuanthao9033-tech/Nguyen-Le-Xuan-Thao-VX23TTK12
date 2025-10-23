using IphoneStoreBE.Common.Models;
using IphoneStoreBE.Entities;
using IphoneStoreBE.VModels;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace IphoneStoreBE.Services

{
    public interface IAuthService
    {
        // ======================================================
        // 🔹 Xác thực & Phiên đăng nhập
        // ======================================================

        /// <summary>
        /// Đăng ký người dùng mới
        /// </summary>
        Task<ResponseResult> RegisterAsync(RegisterVModel model);

        /// <summary>
        /// Đăng nhập người dùng (Cookie + JWT song song)
        /// </summary>
        Task<ResponseResult<LoginResultVModel>> LoginAsync(LoginVModel model, HttpContext httpContext);

        /// <summary>
        /// Sinh JWT token cho user
        /// </summary>
        string GenerateJwtToken(User user);

        /// <summary>
        /// Đăng xuất, xóa session/cookie hiện tại
        /// </summary>
        Task<ResponseResult> LogoutAsync(HttpContext httpContext);

        /// <summary>
        /// Kiểm tra trạng thái xác thực hiện tại (cookie/token)
        /// </summary>
        Task<AuthStatusVModel> GetAuthStatusAsync(ClaimsPrincipal user, HttpContext httpContext);

        /// <summary>
        /// Cập nhật mật khẩu của người dùng hiện tại
        /// </summary>
        Task<ResponseResult> UpdatePasswordAsync(UpdatePasswordVModel model, HttpContext httpContext);

        // ======================================================
        // 🔹 Quản lý người dùng (Admin)
        // ======================================================

        Task<ResponseResult<List<UserVModel>>> GetAllUsersAsync();
        Task<ResponseResult<UserVModel>> GetUserByIdAsync(int id);
        Task<ResponseResult<UserVModel>> AddUserAsync(AddUserVModel model);
        Task<ResponseResult<UserVModel>> UpdateUserAsync(int id, UpdateUserVModel model);
        Task<ResponseResult> DeleteUserAsync(int id);
    }
}
