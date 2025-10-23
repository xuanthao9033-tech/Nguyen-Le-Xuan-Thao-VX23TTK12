namespace IphoneStoreBE.VModels
{
    // 🔹 CreateVModel: ViewModel để tạo mới sản phẩm (dùng khi thêm sản phẩm)
    public class ProductCreateVModel
    {
        public string ProductName { get; set; } = null!;
        public string Sku { get; set; } = null!;
        public decimal Price { get; set; }
        public string? SpecificationDescription { get; set; }
        public string? Warranty { get; set; }
        public string? ProductType { get; set; }
        public string? Color { get; set; }
        public string? Capacity { get; set; }

        // ✅ Thêm: Link ảnh sản phẩm
        public string? ImageUrl { get; set; }

        public int CategoryId { get; set; }

        // ✅ Thêm thuộc tính IsActive để tương thích với mapping
        public bool? IsActive { get; set; }
    }

    // 🔹 UpdateVModel: ViewModel để cập nhật sản phẩm (dùng khi edit)
    public class ProductUpdateVModel
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = null!;
        public string Sku { get; set; } = null!;
        public decimal Price { get; set; }
        public string? SpecificationDescription { get; set; }
        public string? Warranty { get; set; }
        public string? ProductType { get; set; }
        public string? Color { get; set; }
        public string? Capacity { get; set; }

        // ✅ Thêm: link ảnh có thể cập nhật
        public string? ImageUrl { get; set; }

        public bool? IsActive { get; set; }
        public int CategoryId { get; set; }
    }

    // 🔹 GetVModel: ViewModel trả dữ liệu ra FE
    public class ProductGetVModel
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = null!;
        public string Sku { get; set; } = null!;
        public decimal Price { get; set; }
        public string? SpecificationDescription { get; set; }
        public string? Warranty { get; set; }
        public string? ProductType { get; set; }
        public string? Color { get; set; }
        public string? Capacity { get; set; }

        // ✅ Thêm thuộc tính hình ảnh (bên FE cần dùng)
        public string? ImageUrl { get; set; }

        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public int? CategoryId { get; set; }

        // ✅ Thêm thuộc tính CategoryName được mapping sử dụng
        public string? CategoryName { get; set; }
    }
}
