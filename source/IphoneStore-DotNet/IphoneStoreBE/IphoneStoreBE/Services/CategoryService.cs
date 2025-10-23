using IphoneStoreBE.Common.Models;
using IphoneStoreBE.Context;
using IphoneStoreBE.Entities;
using IphoneStoreBE.Mappings;
using IphoneStoreBE.Services.IServices;
using IphoneStoreBE.VModels;
using Microsoft.EntityFrameworkCore;

namespace IphoneStoreBE.Services
{
    public class CategoryService : ICategoryService
    {
        // Gọi context để truy cập dữ liệu từ cơ sở dữ liệu
        private readonly IphoneStoreContext _context;

        // Constructor nhận DbContext
        public CategoryService(IphoneStoreContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // ----- [Validation phương thức tạo mới danh mục] -------------------------
        public static string ValidateCreate(CategoryCreateVModel model)
        {
            // Kiểm tra tên danh mục không được để trống
            if (string.IsNullOrWhiteSpace(model.CategoryName))
                return "Tên danh mục là bắt buộc.";
            // Kiểm tra độ dải tên danh mục (tối đa 50 ký tự)
            if (model.CategoryName.Length > 50)
                return "Tên danh mục phải ít hơn 50 ký tự.";
            
            // Trả về chuỗi rỗng nếu không có lỗi
            return string.Empty; 
        }
        // ----- [Validation phương thức tạo mới danh mục] -------------------------
        public static string ValidateUpdate(CategoryUpdateVModel model)
        {
            // Kiểm tra Id có giá trị hợp lệ
            if (model.Id <= 0)
                return "Id phải lớn hơn 0.";
            // Kiem tra tên danh mục không được để trống
            if (string.IsNullOrWhiteSpace(model.CategoryName))
                return "Tên danh mục là bắt buộc.";
            // Kiểm tra độ dài tên danh mục (tối đa 50 ký tự)
            if (model.CategoryName.Length > 50)
                return "Tên danh mục phải ít hơn 50 ký tự.";
            if (model.IsActive == null)
                return "IsActive phải là true hoặc false.";
            
            // Trả về chuỗi rỗng nếu không có lỗi
            return string.Empty;
        }

        // [01.] --- Phương thức tạo mới danh mục
        public async Task<ResponseResult> CreateAsync(CategoryCreateVModel model)
        {
            try
            {
                // B1: Kiểm tra dữ liệu đầu vào hợp lệ không?
                var validationResult = ValidateCreate(model);
                if (!string.IsNullOrEmpty(validationResult))
                    return ResponseResult.Fail(validationResult);

                // B2: Kiểm tra xem danh mục đã tồn tại chưa (so sánh không phân biệt chữ hoa/thường)
                var existingCategoty = await _context.Categories
                    .AnyAsync(c => !string.IsNullOrEmpty(c.CategoryName) 
                        && !string.IsNullOrEmpty(model.CategoryName)
                        && c.CategoryName.Trim().ToLower() == model.CategoryName.Trim().ToLower()); 
                if (existingCategoty)
                    return ResponseResult.Fail($"Danh mục '{model.CategoryName}' đã tồn tại.");

                // B3: Chuyển VModel sang Entity
                var category = model.ToEntity();

                // B4: Thêm mới danh mục vào cơ sở dữ liệu
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                // B5: Trả về thông báo thành công.
                return ResponseResult.Ok($"Đã tạo danh mục thành công!");
            }
            catch (Exception ex) {
                return ResponseResult.Fail($"Đã có lỗi phát sinh khi tạo danh mục: {ex.Message}");
            }
        }

        // [02.] --- Phương thức xóa danh mục theo ID
        public async Task<ResponseResult> DeleteAsync(int id)
        {
            try
            {
                // B1: Tìm danh mục theo Id.
                var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
                if (category == null)
                    return ResponseResult.Fail($"Không tìm thấy danh mục với Id: {id}.");

                // B2: Xóa danh mục nếu tồn tại.
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                // B3: Trả về thông báo thành công.
                return ResponseResult.Ok($"Đã xóa danh mục với Id: {id} thành công.");
            }
            catch (Exception ex) {
                return ResponseResult.Fail($"Có lỗi phát sinh: {ex.Message}");
            }
        }

        // [03.] --- Phương thức lấy tất cả danh mục
        public async Task<ResponseResult<List<CategoryGetVModel>>> GetAllAsync()
        {
            var categories = await _context.Categories
                .OrderBy(c => c.Id)
                .Select(c => c.ToGetVModel())
                .ToListAsync();
            return ResponseResult<List<CategoryGetVModel>>.Ok(categories, "Lấy danh sách danh mục thành công!");
        }

        // [04.] --- Phương thức lấy danh mục theo ID
        public async Task<ResponseResult<CategoryGetVModel?>> GetByIdAsync(int id)
        {
            var categories = await _context.Categories
                .OrderBy(c => c.Id)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (categories == null)
                return ResponseResult<CategoryGetVModel?>.Fail($"Không tìm thấy danh mục với Id: {id}.");

            // B3: Ánh xạ sang UserGetVModel
            var categoryVModel = categories?.ToGetVModel();


            return ResponseResult<CategoryGetVModel?>.Ok(categoryVModel, "Lấy danh mục theo Id thành công!");
        }

        // [05.] --- Phương thức cập nhật danh mục
        public async Task<ResponseResult> UpdateAsync(CategoryUpdateVModel model)
        {
            try
            {
                // B1: Kiểm tra dữ liệu đầu vào hợp lệ không?
                var validationResult = ValidateUpdate(model);
                if (!string.IsNullOrEmpty(validationResult))
                    return ResponseResult.Fail(validationResult);

                // B2: Tìm danh mục theo ID
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Id == model.Id);
                if (category == null)
                    return ResponseResult.Fail($"Không tìm thấy danh mục với Id: {model.Id}.");

                // B3: Kiểm tra xem tên danh mục đã tồn tại chưa? (trừ tên danh mục hiện tại)
                var existingCategory = await _context.Categories
                    .AnyAsync(c => 
                        !string.IsNullOrEmpty(c.CategoryName) && !string.IsNullOrEmpty(model.CategoryName) 
                        && c.CategoryName.ToLower() == model.CategoryName.ToLower() 
                        && c.Id != model.Id);
                if (existingCategory)
                    return ResponseResult.Fail($"Danh mục '{model.CategoryName}' đã tồn tại.");

                // B4: Cập nhật thông tin danh mục từ model & Lưu thay đổi vào DbContext
                category.UpdateEntity(model);
                await _context.SaveChangesAsync();

                // B5: Trả về thông báo thành công.
                var categoryVM = category.ToGetVModel(); 
                return ResponseResult.Ok($"Đã cập nhật danh mục thành công!");
            }
            catch (Exception ex)
            {
                return ResponseResult.Fail($"Đã có lỗi phát sinh khi cập nhật danh mục: {ex.Message}");
            }
        }
    }
}
