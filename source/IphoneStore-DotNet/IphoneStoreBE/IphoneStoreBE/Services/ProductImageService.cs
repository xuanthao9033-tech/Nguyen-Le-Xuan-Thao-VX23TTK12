using IphoneStoreBE.Common.Models;
using IphoneStoreBE.Context;
using IphoneStoreBE.Entities;
using IphoneStoreBE.Mappings;
using IphoneStoreBE.Services.IServices;
using IphoneStoreBE.VModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace IphoneStoreBE.Services
{
    public class ProductImageService : IProductImageService
    {
        private readonly IphoneStoreContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProductImageService(IphoneStoreContext context, IWebHostEnvironment environment)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        // ----- [Validation phương thức tạo mới ảnh] ------------------------- 
        public static string ValidateCreate(ProductImageCreateVModel model, IFormFile imageFile)
        {
            if (model.ProductId <= 0)
                return "ProductId phải lớn hơn 0.";
            if (!string.IsNullOrWhiteSpace(model.AltText) && model.AltText.Length > 200)
                return "AltText phải ít hơn 200 ký tự.";
            if (imageFile == null || imageFile.Length == 0)
                return "File ảnh là bắt buộc.";
            if (imageFile.Length > 5 * 1024 * 1024) // 5MB
                return "File ảnh phải nhỏ hơn 5MB.";
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return "File phải là JPG, JPEG hoặc PNG.";
            return string.Empty;
        }

        // ----- [Validation phương thức cập nhật ảnh] ------------------------- 
        public static string ValidateUpdate(ProductImageUpdateVModel model)
        {
            if (model.Id <= 0)
                return "Id phải lớn hơn 0.";
            if (!string.IsNullOrWhiteSpace(model.AltText) && model.AltText.Length > 200)
                return "AltText phải ít hơn 200 ký tự.";
            if (model.IsActive == null)
                return "IsActive phải là true hoặc false.";
            return string.Empty;
        }

        // [01.] --- Phương thức tạo mới ảnh sản phẩm
        public async Task<ResponseResult> CreateAsync(ProductImageCreateVModel model, IFormFile imageFile)
        {
            try
            {
                var validationResult = ValidateCreate(model, imageFile);
                if (!string.IsNullOrEmpty(validationResult))
                    return ResponseResult.Fail(validationResult);

                // ✅ Kiểm tra Product tồn tại và lấy SKU
                var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == model.ProductId);
                if (product == null)
                    return ResponseResult.Fail($"Sản phẩm với Id: {model.ProductId} không tồn tại.");

                // ✅ Đếm số lượng ảnh hiện có của sản phẩm này
                var imageCount = await _context.ProductImages
                    .CountAsync(pi => pi.ProductId == model.ProductId);

                // Upload file local
                var uploadsDir = Path.Combine(_environment.WebRootPath, "images/products");
                Directory.CreateDirectory(uploadsDir);
                
                // 🔧 SỬ DỤNG SKU THAY VÌ GUID
                var extension = Path.GetExtension(imageFile.FileName);
                var fileName = imageCount == 0 
                    ? $"{product.Sku}{extension}"  // Ảnh đầu tiên: SKU.png
                    : $"{product.Sku}_{imageCount + 1}{extension}";  // Ảnh thứ 2+: SKU_2.png
                
                var filePath = Path.Combine(uploadsDir, fileName);
                
                // Xóa file nếu trùng tên
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }
                
                var imageUrl = $"/images/products/{fileName}";

                // Map sang Entity và save
                var productImage = model.ToEntity(imageUrl);
                _context.ProductImages.Add(productImage);
                await _context.SaveChangesAsync();

                return ResponseResult.Ok($"Đã tạo ảnh sản phẩm thành công! File: {fileName}");
            }
            catch (Exception ex)
            {
                return ResponseResult.Fail($"Đã có lỗi phát sinh khi tạo ảnh: {ex.Message}");
            }
        }

        // [02.] --- Phương thức xóa ảnh theo ID
        public async Task<ResponseResult> DeleteAsync(int id)
        {
            try
            {
                var productImage = await _context.ProductImages.FirstOrDefaultAsync(pi => pi.Id == id);
                if (productImage == null)
                    return ResponseResult.Fail($"Không tìm thấy ảnh với Id: {id}.");

                // Xóa file local nếu tồn tại
                var fullPath = Path.Combine(_environment.WebRootPath, productImage.ImageUrl.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }

                _context.ProductImages.Remove(productImage);
                await _context.SaveChangesAsync();

                return ResponseResult.Ok($"Đã xóa ảnh với Id: {id} thành công.");
            }
            catch (Exception ex)
            {
                return ResponseResult.Fail($"Có lỗi phát sinh: {ex.Message}");
            }
        }

        // [03.] --- Phương thức lấy tất cả ảnh (filter theo ProductId nếu có)
        public async Task<ResponseResult<List<ProductImageGetVModel>>> GetAllAsync(int? productId = null)
        {
            var query = _context.ProductImages.AsQueryable();
            if (productId.HasValue)
                query = query.Where(pi => pi.ProductId == productId);

            var productImages = await query
                .OrderBy(pi => pi.Id)
                .Select(pi => pi.ToGetVModel())
                .ToListAsync();
            return ResponseResult<List<ProductImageGetVModel>>.Ok(productImages, "Lấy danh sách ảnh thành công!");
        }

        // [04.] --- Phương thức lấy ảnh theo ID
        public async Task<ResponseResult<ProductImageGetVModel?>> GetByIdAsync(int id)
        {
            var productImage = await _context.ProductImages
                .FirstOrDefaultAsync(pi => pi.Id == id);

            if (productImage == null)
                return ResponseResult<ProductImageGetVModel?>.Fail($"Không tìm thấy ảnh với Id: {id}.");

            var productImageVModel = productImage.ToGetVModel();
            return ResponseResult<ProductImageGetVModel?>.Ok(productImageVModel, "Lấy ảnh theo Id thành công!");
        }

        // [05.] --- Phương thức cập nhật ảnh
        public async Task<ResponseResult> UpdateAsync(ProductImageUpdateVModel model)
        {
            try
            {
                var validationResult = ValidateUpdate(model);
                if (!string.IsNullOrEmpty(validationResult))
                    return ResponseResult.Fail(validationResult);

                var productImage = await _context.ProductImages.FirstOrDefaultAsync(pi => pi.Id == model.Id);
                if (productImage == null)
                    return ResponseResult.Fail($"Không tìm thấy ảnh với Id: {model.Id}.");

                productImage.UpdateEntity(model);
                await _context.SaveChangesAsync();

                return ResponseResult.Ok($"Đã cập nhật ảnh thành công!");
            }
            catch (Exception ex)
            {
                return ResponseResult.Fail($"Đã có lỗi phát sinh khi cập nhật ảnh: {ex.Message}");
            }
        }
    }
}