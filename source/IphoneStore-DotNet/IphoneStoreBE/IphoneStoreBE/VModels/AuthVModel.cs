namespace IphoneStoreBE.VModels
{
    // ============================================================
    // 🔹 ViewModel: Kiểm tra trạng thái đăng nhập
    // ============================================================
    public class AuthStatusVModel
    {
        public bool IsAuthenticated { get; set; }
        public int? UserId { get; set; }            // ID người dùng hiện tại
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? Message { get; set; }

        public string? Token { get; set; }
    }

    // ============================================================
    // 🔹 ViewModel: Đăng ký tài khoản
    // ============================================================
    public class RegisterVModel
    {
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool Gender { get; set; }                // True = Nam, False = Nữ
        public string? PhoneNumber { get; set; }
        public string? UserAddress { get; set; }
        public int? RoleId { get; set; }               // ✅ Thêm RoleId (nullable để cho phép không chọn)
    }

    // ============================================================
    // 🔹 ViewModel: Kết quả đăng nhập (trả về cho FE)
    // ============================================================
    public class LoginResultVModel
    {
        public int Id { get; set; }
        public string UserName { get; set; } = null!;
        public string Role { get; set; } = null!;

        // ✅ Bổ sung 2 trường còn thiếu
        public string Email { get; set; } = null!;
        public string Token { get; set; } = null!;
    }

    // ============================================================
    // 🔹 ViewModel: Đăng nhập
    // ============================================================
    public class LoginVModel
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? Token { get; set; }
    }

    // ============================================================
    // 🔹 ViewModel: Trả về thông tin người dùng
    // ============================================================
    public class UserVModel
    {
        public int Id { get; set; }
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public bool Gender { get; set; }                  // True = Nam, False = Nữ
        public string? PhoneNumber { get; set; }
        public string? UserAddress { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string RoleName { get; set; } = null!;
    }

    // ============================================================
    // 🔹 ViewModel: Thêm người dùng mới (dành cho Admin)
    // ============================================================
    public class AddUserVModel
    {
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool Gender { get; set; }
        public string? PhoneNumber { get; set; }
        public string? UserAddress { get; set; }
        public bool IsActive { get; set; } = true;
        public int RoleId { get; set; }
    }

    // ============================================================
    // 🔹 ViewModel: Cập nhật thông tin người dùng
    // ============================================================
    public class UpdateUserVModel
    {
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public bool Gender { get; set; }
        public string? PhoneNumber { get; set; }
        public string? UserAddress { get; set; }
        public bool? IsActive { get; set; }
        public int RoleId { get; set; }
    }

    // ============================================================
    // 🔹 ViewModel: Cập nhật mật khẩu người dùng
    // ============================================================
    public class UpdatePasswordVModel
    {
        public string CurrentPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
        public string ConfirmNewPassword { get; set; } = null!;
    }
}
