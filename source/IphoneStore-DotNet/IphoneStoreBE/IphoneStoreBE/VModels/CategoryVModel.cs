using System;

namespace IphoneStoreBE.VModels
{
    // 🟩 CREATE CATEGORY VIEWMODEL
    // ➤ Dùng khi tạo mới danh mục sản phẩm
    public class CategoryCreateVModel
    {
        /// <summary>
        /// Tên danh mục sản phẩm
        /// </summary>
        public string? CategoryName { get; set; }
        
        /// <summary>
        /// Trạng thái hoạt động (mặc định: true)
        /// </summary>
        public bool? IsActive { get; set; } = true;
    }

    // 🟨 UPDATE CATEGORY VIEWMODEL
    // ➤ Dùng khi cập nhật danh mục sản phẩm
    public class CategoryUpdateVModel
    {
        /// <summary>
        /// ID của danh mục cần cập nhật
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Tên danh mục mới (nếu có thay đổi)
        /// </summary>
        public string? CategoryName { get; set; }

        /// <summary>
        /// Trạng thái hoạt động
        /// </summary>
        public bool? IsActive { get; set; }
    }

    // 🟦 GET CATEGORY VIEWMODEL
    // ➤ Trả dữ liệu danh mục cho FE hiển thị
    public class CategoryGetVModel
    {
        /// <summary>
        /// ID danh mục
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Tên danh mục
        /// </summary>
        public string? CategoryName { get; set; }

        /// <summary>
        /// Trạng thái hoạt động
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Ngày tạo
        /// </summary>
        public DateTime? CreatedDate { get; set; }

        /// <summary>
        /// Ngày cập nhật gần nhất
        /// </summary>
        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// Người tạo
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Người cập nhật
        /// </summary>
        public string? UpdatedBy { get; set; }
    }
}
