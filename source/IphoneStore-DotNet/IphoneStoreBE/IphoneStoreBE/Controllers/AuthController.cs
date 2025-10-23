using IphoneStoreBE.Common.Models;
using IphoneStoreBE.Services;
using IphoneStoreBE.Services.IServices;
using IphoneStoreBE.VModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IphoneStoreBE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // ======================================================
        // 🟩 Đăng ký tài khoản mới
        // ======================================================
        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterVModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseResult.Fail("Dữ liệu không hợp lệ."));

            var result = await _authService.RegisterAsync(model);
            return result.Success
                ? Ok(result)
                : BadRequest(result);
        }

        // ======================================================
        // 🟨 Đăng nhập
        // ======================================================
        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginVModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseResult<LoginResultVModel>.Fail("Dữ liệu không hợp lệ."));

            var result = await _authService.LoginAsync(model, HttpContext);
            return result.Success
                ? Ok(result)
                : Unauthorized(result);
        }

        // ======================================================
        // ⛔ Đăng xuất
        // ======================================================
        [HttpPost("Logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var result = await _authService.LogoutAsync(HttpContext);
            return result.Success
                ? Ok(result)
                : BadRequest(result);
        }

        // ======================================================
        // 🔎 Kiểm tra trạng thái đăng nhập
        // ======================================================
        [HttpGet("Status")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAuthStatus()
        {
            var status = await _authService.GetAuthStatusAsync(User, HttpContext);
            return Ok(status);
        }

        // ======================================================
        // 🚫 Access Denied
        // ======================================================
        [HttpGet("AccessDenied")]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return Unauthorized(ResponseResult.Fail("Bạn không có quyền truy cập tài nguyên này."));
        }

        // ======================================================
        // 🔑 Đổi mật khẩu người dùng hiện tại
        // ======================================================
        [HttpPut("UpdatePassword")]
        [Authorize]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordVModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseResult.Fail("Dữ liệu không hợp lệ."));

            var result = await _authService.UpdatePasswordAsync(model, HttpContext);
            return result.Success
                ? Ok(result)
                : BadRequest(result);
        }

        // ======================================================
        // 👥 Quản lý người dùng (Admin)
        // ======================================================
        [HttpGet("Users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var result = await _authService.GetAllUsersAsync();
            return result.Success
                ? Ok(result)
                : BadRequest(result);
        }

        [HttpGet("Users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var result = await _authService.GetUserByIdAsync(id);
            return result.Success
                ? Ok(result)
                : NotFound(result);
        }

        [HttpPost("Users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddUser([FromBody] AddUserVModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseResult<UserVModel>.Fail("Dữ liệu không hợp lệ."));

            var result = await _authService.AddUserAsync(model);
            return result.Success
                ? Ok(result)
                : BadRequest(result);
        }

        [HttpPut("Users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserVModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ResponseResult<UserVModel>.Fail("Dữ liệu không hợp lệ."));

            var result = await _authService.UpdateUserAsync(id, model);
            return result.Success
                ? Ok(result)
                : BadRequest(result);
        }

        [HttpDelete("Users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _authService.DeleteUserAsync(id);
            return result.Success
                ? Ok(result)
                : BadRequest(result);
        }
    }
}
