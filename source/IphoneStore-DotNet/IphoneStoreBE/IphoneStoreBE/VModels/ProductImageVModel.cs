namespace IphoneStoreBE.VModels
{
    // CreateVModel: ViewModel để tạo mới ảnh sản phẩm (không có ImageUrl vì generate từ upload)
    public class ProductImageCreateVModel
    {
        public string? AltText { get; set; }
        public int ProductId { get; set; }
    }

    // UpdateVModel: ViewModel để cập nhật ảnh sản phẩm (metadata)
    public class ProductImageUpdateVModel
    {
        public int Id { get; set; }
        public string? AltText { get; set; }
        public bool? IsActive { get; set; }
    }

    // GetVModel: ViewModel để lấy thông tin ảnh sản phẩm
    public class ProductImageGetVModel
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = null!;
        public string? AltText { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public int? ProductId { get; set; }
    }
}