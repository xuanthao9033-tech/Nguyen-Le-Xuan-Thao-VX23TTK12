using IphoneStoreBE.Entities;
using IphoneStoreBE.VModels;

namespace IphoneStoreBE.Mappings
{
    public static class CartExtensions
    {
        // 🔹 1. Chuyển từ CartCreateVModel + userId sang Entity Cart
        public static Cart ToEntity(this CartCreateVModel model, int userId)
        {
            return new Cart
            {
                ProductId = model.ProductId,
                UserId = userId,
                Quantity = model.Quantity,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "User"
            };
        }

        // 🔹 2. Cập nhật Entity từ CartUpdateVModel
        public static void UpdateEntity(this Cart entity, CartUpdateVModel model)
        {
            // Bảo vệ null-check
            if (model == null || entity == null)
                return;

            entity.Quantity = model.Quantity;
            if (model.IsActive.HasValue)
                entity.IsActive = model.IsActive.Value;

            entity.UpdatedDate = DateTime.UtcNow;
            entity.UpdatedBy = "User";
        }

        // 🔹 3. Convert từ Entity sang ViewModel (trả dữ liệu ra API)
        public static CartGetVModel ToGetVModel(this Cart entity)
        {
            if (entity == null)
                return null!;

            return new CartGetVModel
            {
                Id = entity.Id,
                ProductId = entity.ProductId,
                UserId = entity.UserId,
                Quantity = entity.Quantity,
                IsActive = entity.IsActive,
                CreatedDate = entity.CreatedDate,
                UpdatedDate = entity.UpdatedDate,
                CreatedBy = entity.CreatedBy,
                UpdatedBy = entity.UpdatedBy
            };
        }
    }
}
