using System;
using System.Collections.Generic;

namespace IphoneStoreBE.VModels
{
    // ============================================================
    // 🟢 MODEL: Tạo đơn hàng (FE gửi lên khi thanh toán)
    // ============================================================
    public class OrderCreateVModel
    {
        public int UserId { get; set; }

        // 👤 Thông tin người nhận
        public string Recipient { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AddressDetailRecipient { get; set; } = string.Empty;

        // 🏙️ Địa chỉ giao hàng (tùy chọn)
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;

        // 💳 Thanh toán
        public string PaymentMethod { get; set; } = string.Empty; // COD / BANK
        public decimal ShippingPrice { get; set; }
    }

    // ============================================================
    // 🟡 MODEL: Cập nhật đơn hàng (Admin)
    // ============================================================
    public class OrderUpdateVModel
    {
        public int Id { get; set; } // ✅ đã đổi từ OrderId → Id
        public string OrderStatus { get; set; } = string.Empty;
    }

    // ============================================================
    // 🔵 MODEL: Chi tiết từng sản phẩm trong đơn hàng
    // ============================================================
    public class OrderDetailGetVModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImage { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        // Tổng tiền = Quantity * Price
        public decimal TotalPrice => Quantity * Price;
    }

    // ============================================================
    // 🟣 MODEL: Địa chỉ giao hàng (hiển thị ra cho người dùng)
    // ============================================================
    public class OrderAddressGetVModel
    {
        public string Recipient { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string AddressDetailRecipient { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Ward { get; set; } = string.Empty;
    }

    // ============================================================
    // 🟤 MODEL: Thông tin đơn hàng đầy đủ để hiển thị chi tiết
    // ============================================================
    public class OrderGetVModel
    {
        public int Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }

        public decimal Total { get; set; }
        public decimal ShippingPrice { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;

        // Tên người đặt / người dùng liên quan
        public string UserName { get; set; } = string.Empty;

        // 🏡 Thông tin địa chỉ giao hàng
        public OrderAddressGetVModel? OrderAddress { get; set; }

        // 🧾 Danh sách sản phẩm trong đơn
        public List<OrderDetailGetVModel> OrderDetails { get; set; } = new();
    }
}
