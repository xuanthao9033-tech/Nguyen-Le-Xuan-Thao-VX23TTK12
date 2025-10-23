namespace IphoneStoreBE.VModels
{
    // 🟩 Thêm sản phẩm vào giỏ hàng
    public class CartCreateVModel
    {
        public int Quantity { get; set; }
        public int ProductId { get; set; }
        public int? UserId { get; set; }
    }

    // 🟦 Cập nhật số lượng hoặc trạng thái
    public class CartUpdateVModel
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public bool? IsActive { get; set; }
    }

    // 🟨 Hiển thị giỏ hàng
    public class CartGetVModel
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public int? ProductId { get; set; }
        public int? UserId { get; set; }

        // 👇 Thông tin sản phẩm
        public string? ProductName { get; set; }
        public decimal? Price { get; set; }
        public string? ImageUrl { get; set; }
    }
}
