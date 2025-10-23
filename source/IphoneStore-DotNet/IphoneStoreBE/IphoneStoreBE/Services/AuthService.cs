using IphoneStoreBE.Common.Models;
using IphoneStoreBE.Context;
using IphoneStoreBE.Entities;
using IphoneStoreBE.Services.IServices;
using IphoneStoreBE.VModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IphoneStoreBE.Services
{
    public class AuthService : IAuthService
    {
        private readonly IphoneStoreContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(IphoneStoreContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // ======================================================
        // 🟩 Đăng ký
        // ======================================================
        public async Task<ResponseResult> RegisterAsync(RegisterVModel model)
        {
            try
            {
                Console.WriteLine($"🔍 Starting registration for email: {model.Email}");
                
                var email = model.Email.Trim().ToLower();
                
                // Kiểm tra email đã tồn tại
                var emailExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == email);
                if (emailExists)
                {
                    Console.WriteLine($"⚠️ Email already exists: {email}");
                    return ResponseResult.Fail("Email đã tồn tại.");
                }

                Console.WriteLine($"✅ Email is unique: {email}");

                // ✅ Xử lý RoleId: nếu không có thì dùng role "User" mặc định
                int roleId;
                if (model.RoleId.HasValue && model.RoleId.Value > 0)
                {
                    // Kiểm tra RoleId có tồn tại không
                    var roleExists = await _context.Roles.AnyAsync(r => r.Id == model.RoleId.Value);
                    if (!roleExists)
                    {
                        return ResponseResult.Fail($"Role với Id {model.RoleId.Value} không tồn tại.");
                    }
                    roleId = model.RoleId.Value;
                    Console.WriteLine($"✅ Using selected RoleId: {roleId}");
                }
                else
                {
                    // Lấy role "User" mặc định
                    var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "User");
                    if (defaultRole == null)
                    {
                        Console.WriteLine("❌ Default role 'User' not found in database");
                        
                        // Tạo role mặc định nếu chưa có
                        Console.WriteLine("🔧 Creating default roles...");
                        var adminRole = new Role { RoleName = "Admin" };
                        var userRole = new Role { RoleName = "User" };
                        
                        _context.Roles.Add(adminRole);
                        _context.Roles.Add(userRole);
                        await _context.SaveChangesAsync();
                        
                        defaultRole = userRole;
                        Console.WriteLine($"✅ Created roles. User RoleId: {defaultRole.Id}");
                    }
                    
                    roleId = defaultRole.Id;
                    Console.WriteLine($"✅ Using default role: User (Id: {roleId})");
                }

                // Tạo user mới
                var user = new User
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    PhoneNumber = model.PhoneNumber,
                    Gender = model.Gender,
                    UserAddress = model.UserAddress,
                    RoleId = roleId,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    IsActive = true
                };

                Console.WriteLine($"📝 Creating user: {user.UserName}, Email: {user.Email}, RoleId: {user.RoleId}");

                _context.Users.Add(user);
                var rowsAffected = await _context.SaveChangesAsync();

                Console.WriteLine($"💾 Rows affected: {rowsAffected}");
                Console.WriteLine($"✅ User created with Id: {user.Id}");

                if (rowsAffected > 0)
                {
                    return ResponseResult.Ok("Đăng ký thành công!");
                }
                else
                {
                    Console.WriteLine("❌ SaveChanges returned 0 rows affected");
                    return ResponseResult.Fail("Không thể lưu user vào database.");
                }
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"❌ Database error: {dbEx.Message}");
                Console.WriteLine($"   Inner exception: {dbEx.InnerException?.Message}");
                return ResponseResult.Fail($"Lỗi database: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ General error: {ex.Message}");
                Console.WriteLine($"   Stack trace: {ex.StackTrace}");
                return ResponseResult.Fail($"Lỗi đăng ký: {ex.Message}");
            }
        }

        // ======================================================
        // 🟨 Đăng nhập
        // ======================================================
        public async Task<ResponseResult<LoginResultVModel>> LoginAsync(LoginVModel model, HttpContext httpContext)
        {
            try
            {
                var user = await _context.Users.Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user == null)
                    return ResponseResult<LoginResultVModel>.Fail("Tài khoản không tồn tại.");

                if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                    return ResponseResult<LoginResultVModel>.Fail("Mật khẩu không đúng.");

                var token = GenerateJwtToken(user);

                httpContext.Session.SetInt32("UserId", user.Id);
                httpContext.Session.SetString("UserEmail", user.Email);
                httpContext.Session.SetString("JwtToken", token);

                var result = new LoginResultVModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Role = user.Role?.RoleName ?? "User",
                    Email = user.Email,
                    Token = token
                };

                return ResponseResult<LoginResultVModel>.SuccessResult(result, "Đăng nhập thành công");
            }
            catch (Exception ex)
            {
                return ResponseResult<LoginResultVModel>.Fail($"Lỗi đăng nhập: {ex.Message}");
            }
        }

        // ======================================================
        // 🔑 Sinh JWT Token
        // ======================================================
        public string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SuperSecretKeyForJwtTokenGeneration12345678"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "User"),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: "IphoneStoreBackend",
                audience: "IphoneStoreFrontend",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ======================================================
        // ⛔ Đăng xuất
        // ======================================================
        public async Task<ResponseResult> LogoutAsync(HttpContext httpContext)
        {
            httpContext.Session.Clear();
            await Task.CompletedTask;
            return ResponseResult.Ok("Đăng xuất thành công.");
        }

        // ======================================================
        // 🔎 Trạng thái đăng nhập
        // ======================================================
        public async Task<AuthStatusVModel> GetAuthStatusAsync(ClaimsPrincipal user, HttpContext httpContext)
        {
            var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdClaim, out int userId);

            var token = httpContext.Session.GetString("JwtToken");

            return await Task.FromResult(new AuthStatusVModel
            {
                IsAuthenticated = user.Identity?.IsAuthenticated ?? false,
                UserId = userId,
                Token = token,
                Message = user.Identity?.IsAuthenticated == true ? "Đã đăng nhập" : "Chưa đăng nhập"
            });
        }

        // ======================================================
        // 🔒 Cập nhật mật khẩu
        // ======================================================
        public async Task<ResponseResult> UpdatePasswordAsync(UpdatePasswordVModel model, HttpContext httpContext)
        {
            try
            {
                var userId = httpContext.Session.GetInt32("UserId");
                if (userId == null)
                    return ResponseResult.Fail("Không tìm thấy phiên đăng nhập.");

                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    return ResponseResult.Fail("Người dùng không tồn tại.");

                if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
                    return ResponseResult.Fail("Mật khẩu hiện tại không đúng.");

                if (model.NewPassword != model.ConfirmNewPassword)
                    return ResponseResult.Fail("Xác nhận mật khẩu không trùng khớp.");

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                user.UpdatedDate = DateTime.Now;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return ResponseResult.Ok("Đổi mật khẩu thành công!");
            }
            catch (Exception ex)
            {
                return ResponseResult.Fail($"Lỗi đổi mật khẩu: {ex.Message}");
            }
        }

        // ======================================================
        // 👥 Quản lý người dùng
        // ======================================================
        public async Task<ResponseResult<List<UserVModel>>> GetAllUsersAsync()
        {
            var users = await _context.Users.Include(u => u.Role)
                .Select(u => new UserVModel
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Email = u.Email,
                    Gender = u.Gender,
                    PhoneNumber = u.PhoneNumber,
                    UserAddress = u.UserAddress,
                    IsActive = u.IsActive,
                    CreatedDate = u.CreatedDate,
                    UpdatedDate = u.UpdatedDate,
                    RoleName = u.Role.RoleName
                }).ToListAsync();

            return ResponseResult<List<UserVModel>>.SuccessResult(users);
        }

        public async Task<ResponseResult<UserVModel>> GetUserByIdAsync(int id)
        {
            var user = await _context.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return ResponseResult<UserVModel>.Fail("Không tìm thấy người dùng.");

            var userVm = new UserVModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Gender = user.Gender,
                PhoneNumber = user.PhoneNumber,
                UserAddress = user.UserAddress,
                IsActive = user.IsActive,
                CreatedDate = user.CreatedDate,
                UpdatedDate = user.UpdatedDate,
                RoleName = user.Role.RoleName
            };

            return ResponseResult<UserVModel>.SuccessResult(userVm);
        }

        public async Task<ResponseResult<UserVModel>> AddUserAsync(AddUserVModel model)
        {
            try
            {
                var role = await _context.Roles.FindAsync(model.RoleId);
                if (role == null)
                    return ResponseResult<UserVModel>.Fail("Không tìm thấy vai trò.");

                var user = new User
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    Gender = model.Gender,
                    PhoneNumber = model.PhoneNumber,
                    UserAddress = model.UserAddress,
                    RoleId = model.RoleId,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now,
                    IsActive = model.IsActive
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var userVm = new UserVModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    RoleName = role.RoleName
                };

                return ResponseResult<UserVModel>.SuccessResult(userVm, "Thêm người dùng thành công");
            }
            catch (Exception ex)
            {
                return ResponseResult<UserVModel>.Fail($"Lỗi thêm người dùng: {ex.Message}");
            }
        }

        public async Task<ResponseResult<UserVModel>> UpdateUserAsync(int id, UpdateUserVModel model)
        {
            try
            {
                var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);
                if (user == null)
                    return ResponseResult<UserVModel>.Fail("Không tìm thấy người dùng.");

                user.UserName = model.UserName;
                user.Email = model.Email;
                user.Gender = model.Gender;
                user.PhoneNumber = model.PhoneNumber;
                user.UserAddress = model.UserAddress;
                user.IsActive = model.IsActive ?? user.IsActive;
                user.RoleId = model.RoleId;
                user.UpdatedDate = DateTime.Now;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                var updatedVm = new UserVModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Gender = user.Gender,
                    PhoneNumber = user.PhoneNumber,
                    UserAddress = user.UserAddress,
                    RoleName = user.Role?.RoleName ?? "User"
                };

                return ResponseResult<UserVModel>.SuccessResult(updatedVm, "Cập nhật thành công");
            }
            catch (Exception ex)
            {
                return ResponseResult<UserVModel>.Fail($"Lỗi cập nhật: {ex.Message}");
            }
        }

        public async Task<ResponseResult> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return ResponseResult.Fail("Không tìm thấy người dùng.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return ResponseResult.Ok("Xóa người dùng thành công.");
        }

        // ======================================================
        // 👑 Quản lý vai trò
        // ======================================================
        public async Task<ResponseResult<List<RoleViewModel>>> GetAllRolesAsync()
        {
            var roles = await _context.Roles
                .Select(r => new RoleViewModel
                {
                    Id = r.Id,
                    RoleName = r.RoleName
                }).ToListAsync();

            return ResponseResult<List<RoleViewModel>>.SuccessResult(roles);
        }

        public async Task<ResponseResult<RoleViewModel>> GetRoleByIdAsync(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
                return ResponseResult<RoleViewModel>.Fail("Không tìm thấy vai trò.");

            var roleVm = new RoleViewModel
            {
                Id = role.Id,
                RoleName = role.RoleName
            };

            return ResponseResult<RoleViewModel>.SuccessResult(roleVm);
        }

        public async Task<ResponseResult<RoleViewModel>> AddRoleAsync(AddRoleVModel model)
        {
            try
            {
                var roleExists = await _context.Roles.AnyAsync(r => r.RoleName == model.RoleName);
                if (roleExists)
                    return ResponseResult<RoleViewModel>.Fail("Vai trò đã tồn tại.");

                var role = new Role
                {
                    RoleName = model.RoleName
                };

                _context.Roles.Add(role);
                await _context.SaveChangesAsync();

                var roleVm = new RoleViewModel
                {
                    Id = role.Id,
                    RoleName = role.RoleName
                };

                return ResponseResult<RoleViewModel>.SuccessResult(roleVm, "Thêm vai trò thành công");
            }
            catch (Exception ex)
            {
                return ResponseResult<RoleViewModel>.Fail($"Lỗi thêm vai trò: {ex.Message}");
            }
        }

        public async Task<ResponseResult<RoleViewModel>> UpdateRoleAsync(int id, UpdateRoleVModel model)
        {
            try
            {
                var role = await _context.Roles.FindAsync(id);
                if (role == null)
                    return ResponseResult<RoleViewModel>.Fail("Không tìm thấy vai trò.");

                role.RoleName = model.RoleName;

                _context.Roles.Update(role);
                await _context.SaveChangesAsync();

                var roleVm = new RoleViewModel
                {
                    Id = role.Id,
                    RoleName = role.RoleName
                };

                return ResponseResult<RoleViewModel>.SuccessResult(roleVm, "Cập nhật vai trò thành công");
            }
            catch (Exception ex)
            {
                return ResponseResult<RoleViewModel>.Fail($"Lỗi cập nhật vai trò: {ex.Message}");
            }
        }

        public async Task<ResponseResult> DeleteRoleAsync(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null)
                return ResponseResult.Fail("Không tìm thấy vai trò.");

            // Kiểm tra xem có người dùng nào đang sử dụng vai trò này không
            var usersWithRole = await _context.Users.AnyAsync(u => u.RoleId == id);
            if (usersWithRole)
                return ResponseResult.Fail("Không thể xóa vai trò đang được sử dụng.");

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();

            return ResponseResult.Ok("Xóa vai trò thành công.");
        }
    }
}
