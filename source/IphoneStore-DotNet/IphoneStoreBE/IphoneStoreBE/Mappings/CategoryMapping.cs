using IphoneStoreBE.Entities;
using IphoneStoreBE.VModels;
using Microsoft.AspNetCore.Http.HttpResults;

namespace IphoneStoreBE.Mappings
{
    public static class CategoryExtensions
    {
        // Chuyển từ CategoryCreateVModel sang Entity Category
        public static Category ToEntity(this CategoryCreateVModel model) {
            return new Category
            {
                CategoryName = model.CategoryName,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "Admin",
            };
        }
        
        // Cập nhật thông tin Entity Category từ CategoryUpdateVModel
        public static void UpdateEntity(this Category entity, CategoryUpdateVModel model) {
            entity.CategoryName = model.CategoryName;
            entity.IsActive = model.IsActive;
            entity.UpdatedDate = DateTime.UtcNow;
            entity.UpdatedBy = "Admin";
        }

        // Chuyển từ Entity Category sang CategoryGetVModel
        public static CategoryGetVModel ToGetVModel(this Category entity) {
            return new CategoryGetVModel
            {
                Id = entity.Id,
                CategoryName = entity.CategoryName,
                IsActive = entity.IsActive,
                CreatedDate = entity.CreatedDate,
                UpdatedDate = entity.UpdatedDate,
                CreatedBy = entity.CreatedBy,
                UpdatedBy = entity.UpdatedBy,
            };
        }
    }
}
