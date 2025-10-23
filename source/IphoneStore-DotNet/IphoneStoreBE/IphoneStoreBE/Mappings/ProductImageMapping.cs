using IphoneStoreBE.Entities;
using IphoneStoreBE.VModels;

namespace IphoneStoreBE.Mappings
{
    public static class ProductImageExtensions
    {
        // Chuyển từ ProductImageCreateVModel + ImageUrl sang Entity ProductImage
        public static ProductImage ToEntity(this ProductImageCreateVModel model, string imageUrl)
        {
            return new ProductImage
            {
                ImageUrl = imageUrl,
                AltText = model.AltText,
                IsActive = true,
                ProductId = model.ProductId,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "Admin",
            };
        }

        // Cập nhật thông tin Entity ProductImage từ ProductImageUpdateVModel
        public static void UpdateEntity(this ProductImage entity, ProductImageUpdateVModel model)
        {
            entity.AltText = model.AltText;
            entity.IsActive = model.IsActive;
            entity.UpdatedDate = DateTime.UtcNow;
            entity.UpdatedBy = "Admin";
        }

        // Chuyển từ Entity ProductImage sang ProductImageGetVModel
        public static ProductImageGetVModel ToGetVModel(this ProductImage entity)
        {
            return new ProductImageGetVModel
            {
                Id = entity.Id,
                ImageUrl = entity.ImageUrl,
                AltText = entity.AltText,
                IsActive = entity.IsActive,
                CreatedDate = entity.CreatedDate,
                UpdatedDate = entity.UpdatedDate,
                CreatedBy = entity.CreatedBy,
                UpdatedBy = entity.UpdatedBy,
                ProductId = entity.ProductId,
            };
        }
    }
}