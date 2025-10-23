using IphoneStoreBE.Common.Models;
using IphoneStoreBE.Context;
using IphoneStoreBE.Entities;
using IphoneStoreBE.Mappings;
using IphoneStoreBE.Services.IServices;
using IphoneStoreBE.VModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IphoneStoreBE.Services
{
    public class ProductService : IProductService
    {
        private readonly IphoneStoreContext _context;

        public ProductService(IphoneStoreContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public static string ValidateCreate(ProductCreateVModel model)
        {
            if (string.IsNullOrWhiteSpace(model.ProductName))
                return "Tên sản phẩm là bắt buộc.";
            if (model.ProductName.Length > 100)
                return "Tên sản phẩm phải ít hơn 100 ký tự.";
            if (string.IsNullOrWhiteSpace(model.Sku))
                return "SKU là bắt buộc.";
            if (model.Sku.Length > 50)
                return "SKU phải ít hơn 50 ký tự.";
            if (model.Price <= 0)
                return "Giá sản phẩm phải lớn hơn 0.";
            if (model.CategoryId <= 0)
                return "CategoryId phải lớn hơn 0.";
            if (!string.IsNullOrWhiteSpace(model.SpecificationDescription) && model.SpecificationDescription.Length > 500)
                return "Mô tả thông số kỹ thuật phải ít hơn 500 ký tự.";
            if (!string.IsNullOrWhiteSpace(model.Warranty) && model.Warranty.Length > 100)
                return "Bảo hành phải ít hơn 100 ký tự.";
            if (!string.IsNullOrWhiteSpace(model.ProductType) && model.ProductType.Length > 50)
                return "Loại sản phẩm phải ít hơn 50 ký tự.";
            if (!string.IsNullOrWhiteSpace(model.Color) && model.Color.Length > 50)
                return "Màu sắc phải ít hơn 50 ký tự.";
            if (!string.IsNullOrWhiteSpace(model.Capacity) && model.Capacity.Length > 50)
                return "Dung lượng phải ít hơn 50 ký tự.";

            return string.Empty;
        }

        public static string ValidateUpdate(ProductUpdateVModel model)
        {
            if (model.Id <= 0)
                return "Id phải lớn hơn 0.";
            if (string.IsNullOrWhiteSpace(model.ProductName))
                return "Tên sản phẩm là bắt buộc.";
            if (model.ProductName.Length > 100)
                return "Tên sản phẩm phải ít hơn 100 ký tự.";
            if (string.IsNullOrWhiteSpace(model.Sku))
                return "SKU là bắt buộc.";
            if (model.Sku.Length > 50)
                return "SKU phải ít hơn 50 ký tự.";
            if (model.Price <= 0)
                return "Giá sản phẩm phải lớn hơn 0.";
            if (model.CategoryId <= 0)
                return "CategoryId phải lớn hơn 0.";
            if (model.IsActive == null)
                return "IsActive phải là true hoặc false.";
            if (!string.IsNullOrWhiteSpace(model.SpecificationDescription) && model.SpecificationDescription.Length > 500)
                return "Mô tả thông số kỹ thuật phải ít hơn 500 ký tự.";
            if (!string.IsNullOrWhiteSpace(model.Warranty) && model.Warranty.Length > 100)
                return "Bảo hành phải ít hơn 100 ký tự.";
            if (!string.IsNullOrWhiteSpace(model.ProductType) && model.ProductType.Length > 50)
                return "Loại sản phẩm phải ít hơn 50 ký tự.";
            if (!string.IsNullOrWhiteSpace(model.Color) && model.Color.Length > 50)
                return "Màu sắc phải ít hơn 50 ký tự.";
            if (!string.IsNullOrWhiteSpace(model.Capacity) && model.Capacity.Length > 50)
                return "Dung lượng phải ít hơn 50 ký tự.";

            return string.Empty;
        }

        public async Task<ResponseResult> CreateAsync(ProductCreateVModel model)
        {
            try
            {
                var validationResult = ValidateCreate(model);
                if (!string.IsNullOrEmpty(validationResult))
                    return ResponseResult.Fail(validationResult);

                var existingProduct = await _context.Products
                    .AnyAsync(p => !string.IsNullOrEmpty(p.Sku)
                        && !string.IsNullOrEmpty(model.Sku)
                        && p.Sku.Trim().ToLower() == model.Sku.Trim().ToLower());
                if (existingProduct)
                    return ResponseResult.Fail($"Sản phẩm với SKU '{model.Sku}' đã tồn tại.");

                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == model.CategoryId);
                if (!categoryExists)
                    return ResponseResult.Fail($"Danh mục với Id: {model.CategoryId} không tồn tại.");

                var product = model.ToEntity();
                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return ResponseResult.Ok($"Đã tạo sản phẩm thành công!");
            }
            catch (Exception ex)
            {
                return ResponseResult.Fail($"Đã có lỗi phát sinh khi tạo sản phẩm: {ex.Message}");
            }
        }

        public async Task<ResponseResult> DeleteAsync(int id)
        {
            try
            {
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
                if (product == null)
                    return ResponseResult.Fail($"Không tìm thấy sản phẩm với Id: {id}.");

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                return ResponseResult.Ok($"Đã xóa sản phẩm với Id: {id} thành công.");
            }
            catch (Exception ex)
            {
                return ResponseResult.Fail($"Có lỗi phát sinh: {ex.Message}");
            }
        }

        public async Task<ResponseResult<List<ProductGetVModel>>> GetAllAsync()
        {
            var products = await _context.Products
                .Include(p => p.Category) // Thêm dòng này
                .OrderBy(p => p.Id)
                .Select(p => p.ToGetVModel())
                .ToListAsync();
            return ResponseResult<List<ProductGetVModel>>.Ok(products, "Lấy danh sách sản phẩm thành công!");
        }

        public async Task<ResponseResult<ProductGetVModel?>> GetByIdAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category) // Thêm dòng này
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return ResponseResult<ProductGetVModel?>.Fail($"Không tìm thấy sản phẩm với Id: {id}.");

            var productVModel = product?.ToGetVModel();

            return ResponseResult<ProductGetVModel?>.Ok(productVModel, "Lấy sản phẩm theo Id thành công!");
        }

        public async Task<ResponseResult> UpdateAsync(ProductUpdateVModel model)
        {
            try
            {
                var validationResult = ValidateUpdate(model);
                if (!string.IsNullOrEmpty(validationResult))
                    return ResponseResult.Fail(validationResult);

                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == model.Id);
                if (product == null)
                    return ResponseResult.Fail($"Không tìm thấy sản phẩm với Id: {model.Id}.");

                var existingProduct = await _context.Products
                    .AnyAsync(p =>
                        !string.IsNullOrEmpty(p.Sku) && !string.IsNullOrEmpty(model.Sku)
                        && p.Sku.ToLower() == model.Sku.ToLower()
                        && p.Id != model.Id);
                if (existingProduct)
                    return ResponseResult.Fail($"Sản phẩm với SKU '{model.Sku}' đã tồn tại.");

                var categoryExists = await _context.Categories.AnyAsync(c => c.Id == model.CategoryId);
                if (!categoryExists)
                    return ResponseResult.Fail($"Danh mục với Id: {model.CategoryId} không tồn tại.");

                product.UpdateEntity(model);
                await _context.SaveChangesAsync();

                var productVM = product.ToGetVModel();
                return ResponseResult.Ok($"Đã cập nhật sản phẩm thành công!");
            }
            catch (Exception ex)
            {
                return ResponseResult.Fail($"Đã có lỗi phát sinh khi cập nhật sản phẩm: {ex.Message}");
            }
        }
    }
}
