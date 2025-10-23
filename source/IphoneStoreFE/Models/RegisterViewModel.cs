namespace IphoneStoreFE.Models
{
    public class RegisterViewModel
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public bool Gender { get; set; } = true;
        public string? PhoneNumber { get; set; }
        public string? UserAddress { get; set; }
        public int RoleId { get; set; } = 2; // Mặc định User
    }
}
