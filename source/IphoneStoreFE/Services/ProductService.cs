using IphoneStoreBE.VModels;
using System.Net.Http.Json;
using System.Text.Json;
using IphoneStoreFE.Models;

namespace IphoneStoreFE.Services
{
    /// <summary>
    /// Service quản lý các thao tác CRUD cho Product
    /// </summary>
    public class ProductService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public ProductService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Lấy tất cả sản phẩm có phân trang
        /// </summary>
        public async Task<ServiceResult<IphoneStoreFE.Models.PagedEntity<ProductGetVModel>>> GetAllProductsAsync(int page = 1, int pageSize = 12)
        {
            try
            {
                var response = await _httpClient.GetAsync($"product?page={page}&pageSize={pageSize}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ServiceResult<IphoneStoreFE.Models.PagedEntity<ProductGetVModel>>.Failure(
                        $"Lỗi API: {response.StatusCode} - {errorContent}");
                }

                var result = await response.Content
                    .ReadFromJsonAsync<IphoneStoreFE.Models.ResponseResult<IphoneStoreFE.Models.PagedEntity<ProductGetVModel>>>(_jsonOptions);

                if (result?.Success == true && result.Data != null)
                {
                    return ServiceResult<IphoneStoreFE.Models.PagedEntity<ProductGetVModel>>.Success(result.Data);
                }

                return ServiceResult<IphoneStoreFE.Models.PagedEntity<ProductGetVModel>>.Failure(
                    result?.Message ?? "Không thể lấy danh sách sản phẩm");
            }
            catch (HttpRequestException ex)
            {
                return ServiceResult<IphoneStoreFE.Models.PagedEntity<ProductGetVModel>>.Failure(
                    $"Lỗi kết nối: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ServiceResult<IphoneStoreFE.Models.PagedEntity<ProductGetVModel>>.Failure(
                    $"Lỗi không xác định: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy sản phẩm theo danh mục
        /// </summary>
        public async Task<ServiceResult<IphoneStoreFE.Models.PagedEntity<ProductGetVModel>>> GetProductsByCategoryAsync(string categoryName, int page = 1, int pageSize = 12)
        {
            try
            {
                var encodedCategoryName = Uri.EscapeDataString(categoryName);
                var response = await _httpClient.GetAsync($"product/category/{encodedCategoryName}?page={page}&pageSize={pageSize}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ServiceResult<IphoneStoreFE.Models.PagedEntity<ProductGetVModel>>.Failure(
                        $"Lỗi API: {response.StatusCode} - {errorContent}");
                }

                var result = await response.Content
                    .ReadFromJsonAsync<IphoneStoreFE.Models.ResponseResult<IphoneStoreFE.Models.PagedEntity<ProductGetVModel>>>(_jsonOptions);

                if (result?.Success == true && result.Data != null)
                {
                    return ServiceResult<IphoneStoreFE.Models.PagedEntity<ProductGetVModel>>.Success(result.Data);
                }

                return ServiceResult<IphoneStoreFE.Models.PagedEntity<ProductGetVModel>>.Failure(
                    result?.Message ?? "Không thể lấy sản phẩm theo danh mục");
            }
            catch (HttpRequestException ex)
            {
                return ServiceResult<IphoneStoreFE.Models.PagedEntity<ProductGetVModel>>.Failure(
                    $"Lỗi kết nối: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ServiceResult<IphoneStoreFE.Models.PagedEntity<ProductGetVModel>>.Failure(
                    $"Lỗi không xác định: {ex.Message}");
            }
        }

        /// <summary>
        /// Tìm kiếm sản phẩm theo từ khóa
        /// </summary>
        public async Task<ServiceResult<IphoneStoreFE.Models.PagedEntity<ProductGetVModel>>> SearchProductsAsync(string keyword, int page = 1, int pageSize = 12)
        {
            try
            {
                var encodedKeyword = Uri.EscapeDataString(keyword);
                var response = await _httpClient.GetAsync($"product/search?keyword={encodedKeyword}&page={page}&pageSize={pageSize}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ServiceResult<IphoneStoreFE.Models.PagedEntity<ProductGetVModel>>.Failure(
                        $"Lỗi API: {response.StatusCode} - {errorContent}");
                }

                var result = await response.Content
                    .ReadFromJsonAsync<IphoneStoreFE.Models.ResponseResult<IphoneStoreFE.Models.PagedEntity<ProductGetVModel>>>(_jsonOptions);

                if (result?.Success == true && result.Data != null)
                {
                    return ServiceResult<IphoneStoreFE.Models.PagedEntity<ProductGetVModel>>.Success(result.Data);
                }

                return ServiceResult<IphoneStoreFE.Models.PagedEntity<ProductGetVModel>>.Failure(
                    result?.Message ?? "Không thể tìm kiếm sản phẩm");
            }
            catch (HttpRequestException ex)
            {
                return ServiceResult<IphoneStoreFE.Models.PagedEntity<ProductGetVModel>>.Failure(
                    $"Lỗi kết nối: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ServiceResult<IphoneStoreFE.Models.PagedEntity<ProductGetVModel>>.Failure(
                    $"Lỗi không xác định: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy chi tiết sản phẩm theo ID
        /// </summary>
        public async Task<ServiceResult<ProductGetVModel>> GetProductByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"product/{id}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ServiceResult<ProductGetVModel>.Failure(
                        $"Lỗi API: {response.StatusCode} - {errorContent}");
                }

                var result = await response.Content
                    .ReadFromJsonAsync<IphoneStoreFE.Models.ResponseResult<ProductGetVModel>>(_jsonOptions);

                if (result?.Success == true && result.Data != null)
                {
                    return ServiceResult<ProductGetVModel>.Success(result.Data);
                }

                return ServiceResult<ProductGetVModel>.Failure(
                    result?.Message ?? "Không thể lấy chi tiết sản phẩm");
            }
            catch (HttpRequestException ex)
            {
                return ServiceResult<ProductGetVModel>.Failure(
                    $"Lỗi kết nối: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ServiceResult<ProductGetVModel>.Failure(
                    $"Lỗi không xác định: {ex.Message}");
            }
        }

        /// <summary>
        /// Tạo sản phẩm mới
        /// </summary>
        public async Task<ServiceResult<ProductGetVModel>> CreateProductAsync(ProductCreateVModel model)
        {
            try
            {
                // Validate input
                var validationResult = ValidateProductCreateModel(model);
                if (!validationResult.IsSuccess)
                {
                    return ServiceResult<ProductGetVModel>.Failure(validationResult.Message);
                }

                var response = await _httpClient.PostAsJsonAsync("product", model, _jsonOptions);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ServiceResult<ProductGetVModel>.Failure(
                        $"Lỗi API: {response.StatusCode} - {errorContent}");
                }

                var result = await response.Content
                    .ReadFromJsonAsync<IphoneStoreFE.Models.ResponseResult<ProductGetVModel>>(_jsonOptions);

                if (result?.Success == true && result.Data != null)
                {
                    return ServiceResult<ProductGetVModel>.Success(result.Data);
                }

                return ServiceResult<ProductGetVModel>.Failure(
                    result?.Message ?? "Không thể tạo sản phẩm");
            }
            catch (HttpRequestException ex)
            {
                return ServiceResult<ProductGetVModel>.Failure(
                    $"Lỗi kết nối: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ServiceResult<ProductGetVModel>.Failure(
                    $"Lỗi không xác định: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật sản phẩm
        /// </summary>
        public async Task<ServiceResult<ProductGetVModel>> UpdateProductAsync(ProductUpdateVModel model)
        {
            try
            {
                // Validate input
                var validationResult = ValidateProductUpdateModel(model);
                if (!validationResult.IsSuccess)
                {
                    return ServiceResult<ProductGetVModel>.Failure(validationResult.Message);
                }

                var response = await _httpClient.PutAsJsonAsync($"product/{model.Id}", model, _jsonOptions);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ServiceResult<ProductGetVModel>.Failure(
                        $"Lỗi API: {response.StatusCode} - {errorContent}");
                }

                var result = await response.Content
                    .ReadFromJsonAsync<IphoneStoreFE.Models.ResponseResult<ProductGetVModel>>(_jsonOptions);

                if (result?.Success == true && result.Data != null)
                {
                    return ServiceResult<ProductGetVModel>.Success(result.Data);
                }

                return ServiceResult<ProductGetVModel>.Failure(
                    result?.Message ?? "Không thể cập nhật sản phẩm");
            }
            catch (HttpRequestException ex)
            {
                return ServiceResult<ProductGetVModel>.Failure(
                    $"Lỗi kết nối: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ServiceResult<ProductGetVModel>.Failure(
                    $"Lỗi không xác định: {ex.Message}");
            }
        }

        /// <summary>
        /// Xóa sản phẩm
        /// </summary>
        public async Task<ServiceResult<bool>> DeleteProductAsync(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return ServiceResult<bool>.Failure("ID sản phẩm không hợp lệ");
                }

                var response = await _httpClient.DeleteAsync($"product/{id}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return ServiceResult<bool>.Failure(
                        $"Lỗi API: {response.StatusCode} - {errorContent}");
                }

                var result = await response.Content
                    .ReadFromJsonAsync<IphoneStoreFE.Models.ResponseResult>(_jsonOptions);

                if (result?.Success == true)
                {
                    return ServiceResult<bool>.Success(true);
                }

                return ServiceResult<bool>.Failure(
                    result?.Message ?? "Không thể xóa sản phẩm");
            }
            catch (HttpRequestException ex)
            {
                return ServiceResult<bool>.Failure(
                    $"Lỗi kết nối: {ex.Message}");
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure(
                    $"Lỗi không xác định: {ex.Message}");
            }
        }

        #region Validation Methods

        /// <summary>
        /// Validate ProductCreateVModel
        /// </summary>
        private ServiceResult<bool> ValidateProductCreateModel(ProductCreateVModel model)
        {
            if (model == null)
            {
                return ServiceResult<bool>.Failure("Dữ liệu sản phẩm không hợp lệ");
            }

            if (string.IsNullOrWhiteSpace(model.ProductName))
            {
                return ServiceResult<bool>.Failure("Tên sản phẩm không được để trống");
            }

            if (string.IsNullOrWhiteSpace(model.Sku))
            {
                return ServiceResult<bool>.Failure("SKU không được để trống");
            }

            if (model.Price <= 0)
            {
                return ServiceResult<bool>.Failure("Giá sản phẩm phải lớn hơn 0");
            }

            if (model.CategoryId <= 0)
            {
                return ServiceResult<bool>.Failure("Danh mục sản phẩm không hợp lệ");
            }

            return ServiceResult<bool>.Success(true);
        }

        /// <summary>
        /// Validate ProductUpdateVModel
        /// </summary>
        private ServiceResult<bool> ValidateProductUpdateModel(ProductUpdateVModel model)
        {
            if (model == null)
            {
                return ServiceResult<bool>.Failure("Dữ liệu sản phẩm không hợp lệ");
            }

            if (model.Id <= 0)
            {
                return ServiceResult<bool>.Failure("ID sản phẩm không hợp lệ");
            }

            if (string.IsNullOrWhiteSpace(model.ProductName))
            {
                return ServiceResult<bool>.Failure("Tên sản phẩm không được để trống");
            }

            if (string.IsNullOrWhiteSpace(model.Sku))
            {
                return ServiceResult<bool>.Failure("SKU không được để trống");
            }

            if (model.Price <= 0)
            {
                return ServiceResult<bool>.Failure("Giá sản phẩm phải lớn hơn 0");
            }

            if (model.CategoryId <= 0)
            {
                return ServiceResult<bool>.Failure("Danh mục sản phẩm không hợp lệ");
            }

            return ServiceResult<bool>.Success(true);
        }

        #endregion
    }
}

