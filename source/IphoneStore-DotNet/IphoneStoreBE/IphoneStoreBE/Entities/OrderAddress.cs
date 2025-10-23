using System;
using System.Collections.Generic;

namespace IphoneStoreBE.Entities
{
    public class OrderAddress
    {
        public int Id { get; set; }

        // 👤 Thông tin người nhận
        public string Recipient { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AddressDetailRecipient { get; set; } = string.Empty;

        // 🏙️ Thông tin địa chỉ chi tiết
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = "—";
        public string Ward { get; set; } = "—";

        // 🔑 Liên kết người dùng
        public int? UserId { get; set; }

        // ⚙️ Trạng thái và audit
        public bool? IsActive { get; set; } = true;
        public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }

        // ======================================================
        // 🧭 Navigation properties
        // ======================================================

        // ✅ Quan hệ 1 - N (một địa chỉ có thể được dùng trong nhiều Order)
        public virtual ICollection<Order> Orders { get; set; } = new HashSet<Order>();

        // ✅ Liên kết với bảng User (nếu cần)
        public virtual User? User { get; set; }
    }
}
