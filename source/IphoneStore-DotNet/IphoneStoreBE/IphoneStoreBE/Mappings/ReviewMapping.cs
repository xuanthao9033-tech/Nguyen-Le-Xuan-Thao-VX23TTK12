using IphoneStoreBE.Entities;
using IphoneStoreBE.VModels;

namespace IphoneStoreBE.Mappings
{
    public static class ReviewExtensions
    {
        // Chuyển từ ReviewCreateVModel + UserId sang Entity Review
        public static Review ToEntity(this ReviewCreateVModel model, int userId)
        {
            return new Review
            {
                Comment = model.Comment,
                IsActive = true,
                ProductId = model.ProductId,
                UserId = userId,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "User",
            };
        }

        // Cập nhật thông tin Entity Review từ ReviewUpdateVModel
        public static void UpdateEntity(this Review entity, ReviewUpdateVModel model)
        {
            entity.Comment = model.Comment;
            entity.IsActive = model.IsActive;
            entity.UpdatedDate = DateTime.UtcNow;
            entity.UpdatedBy = "User";
        }

        // Chuyển từ Entity Review sang ReviewGetVModel
        public static ReviewGetVModel ToGetVModel(this Review entity)
        {
            return new ReviewGetVModel
            {
                Id = entity.Id,
                Comment = entity.Comment,
                IsActive = entity.IsActive,
                CreatedDate = entity.CreatedDate,
                UpdatedDate = entity.UpdatedDate,
                CreatedBy = entity.CreatedBy,
                UpdatedBy = entity.UpdatedBy,
                ProductId = entity.ProductId,
                UserId = entity.UserId,
            };
        }
    }
}