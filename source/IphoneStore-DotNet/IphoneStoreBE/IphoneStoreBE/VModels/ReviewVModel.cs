namespace IphoneStoreBE.VModels
{
    // CreateVModel: ViewModel để tạo mới Review
    public class ReviewCreateVModel
    {
        public string? Comment { get; set; }
        public int ProductId { get; set; }
    }

    // UpdateVModel: ViewModel để cập nhật Review
    public class ReviewUpdateVModel
    {
        public int Id { get; set; }
        public string? Comment { get; set; }
        public bool? IsActive { get; set; }
    }

    // GetVModel: ViewModel để lấy thông tin Review
    public class ReviewGetVModel
    {
        public int Id { get; set; }
        public string? Comment { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public int? ProductId { get; set; }
        public int? UserId { get; set; }
    }
}