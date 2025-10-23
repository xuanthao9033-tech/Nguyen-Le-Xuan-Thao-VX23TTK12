using IphoneStoreBE.Entities;
using IphoneStoreBE.VModels;

namespace IphoneStoreBE.Mappings
{
    public static class ProductMapping
    {
        public static ProductGetVModel ToGetVModel(this Product product)
        {
            return new ProductGetVModel
            {
                Id = product.Id,
                ProductName = product.ProductName,
                Sku = product.Sku,
                Price = product.Price,
                SpecificationDescription = product.SpecificationDescription,
                Warranty = product.Warranty,
                ProductType = product.ProductType,
                Color = product.Color,
                Capacity = product.Capacity,
                ImageUrl = product.ImageUrl,
                IsActive = product.IsActive,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.CategoryName, // Thêm dòng này
                CreatedDate = product.CreatedDate,
                UpdatedDate = product.UpdatedDate,
                CreatedBy = product.CreatedBy,
                UpdatedBy = product.UpdatedBy
            };
        }

        public static Product ToEntity(this ProductCreateVModel model)
        {
            return new Product
            {
                ProductName = model.ProductName,
                Sku = model.Sku,
                Price = model.Price,
                SpecificationDescription = model.SpecificationDescription,
                Warranty = model.Warranty,
                ProductType = model.ProductType,
                Color = model.Color,
                Capacity = model.Capacity,
                ImageUrl = model.ImageUrl,
                IsActive = model.IsActive ?? true,
                CategoryId = model.CategoryId,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                CreatedBy = "Admin"
            };
        }

        public static void UpdateEntity(this Product product, ProductUpdateVModel model)
        {
            product.ProductName = model.ProductName;
            product.Sku = model.Sku;
            product.Price = model.Price;
            product.SpecificationDescription = model.SpecificationDescription;
            product.Warranty = model.Warranty;
            product.ProductType = model.ProductType;
            product.Color = model.Color;
            product.Capacity = model.Capacity;
            product.ImageUrl = model.ImageUrl ?? product.ImageUrl;
            product.IsActive = model.IsActive;
            product.CategoryId = model.CategoryId;
            product.UpdatedDate = DateTime.UtcNow;
            product.UpdatedBy = "Admin";
        }
    }
}
