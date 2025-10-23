using System;
using System.Collections.Generic;

namespace IphoneStoreBE.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal? Total { get; set; }
        public decimal ShippingPrice { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = "Chờ xác nhận";
        public bool IsActive { get; set; } = true;

        // Khóa ngoại
        public int UserId { get; set; }
        public int? OrderAddId { get; set; }

        // Audit fields
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }

        // Navigation
        public User? User { get; set; }
        public virtual OrderAddress? OrderAdd { get; set; }

        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}
