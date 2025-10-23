using IphoneStoreBE.Common.Models;
using IphoneStoreBE.VModels;
using IphoneStoreFE.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using System.Text.Json;

// This is an MVC controller that returns Views (not an API controller)
public class AccountController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IHttpClientFactory httpClientFactory, ILogger<AccountController> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri("https://localhost:7182/api/");
        _logger = logger;
    }

    // [GET] Đăng nhập
    [HttpGet("/Account/Login")]
    public IActionResult Login() => View();

    // [POST] Đăng nhập
    [HttpPost("/Account/Login")]
    public async Task<IActionResult> Login(LoginVModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var response = await _httpClient.PostAsJsonAsync("Auth/Login", model);
            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Sai tài khoản hoặc mật khẩu.";
                return View(model);
            }

            var result = await response.Content.ReadFromJsonAsync<IphoneStoreBE.Common.Models.ResponseResult<LoginResultVModel>>();
            if (result?.Success == true && result.Data != null)
            {
                var userData = result.Data;

                // Lưu session và cookie
                HttpContext.Session.SetInt32("UserId", userData.Id);
                HttpContext.Session.SetString("User", userData.UserName);
                HttpContext.Session.SetString("Token", userData.Token);
                HttpContext.Session.SetString("Role", userData.Role);

                // Tạo cookie đăng nhập
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userData.Id.ToString()),
                    new Claim(ClaimTypes.Name, userData.UserName),
                    new Claim(ClaimTypes.Role, userData.Role)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                HttpContext.Session.SetInt32("UserId", userData.Id);
                _logger.LogInformation("✅ Set Session UserId: {UserId}, Role: {Role}", userData.Id, userData.Role);

                // ✅ Chuyển hướng dựa trên role
                if (userData.Role == "Admin")
                {
                    _logger.LogInformation("✅ Admin logged in, redirecting to /Admin/Products");
                    return RedirectToAction("Products", "Admin");
                }
                else
                {
                    _logger.LogInformation("✅ User logged in, redirecting to Home");
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Error = result?.Message ?? "Đăng nhập thất bại.";
            return View(model);
        }
        catch (Exception ex)
        {
            ViewBag.Error = $"Lỗi đăng nhập: {ex.Message}";
            return View(model);
        }
    }

    // [GET] Đăng ký
    [HttpGet("/Account/Register")]
    public async Task<IActionResult> Register()
    {
        // ✅ Load danh sách Roles từ Backend
        await LoadRolesAsync();
        return View();
    }

    // [POST] Đăng ký
    [HttpPost("/Account/Register")]
    public async Task<IActionResult> Register(RegisterVModel model)
    {
        if (!ModelState.IsValid)
        {
            await LoadRolesAsync(); // ✅ Reload roles nếu validation fail
            return View(model);
        }

        try
        {
            _logger.LogInformation("🔍 Registering user: {Email}", model.Email);

            var response = await _httpClient.PostAsJsonAsync("Auth/Register", model);

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("📡 Register API Response: {Content}", responseContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorResult = await response.Content.ReadFromJsonAsync<IphoneStoreBE.Common.Models.ResponseResult>();
                ViewBag.Error = errorResult?.Message ?? "Đăng ký thất bại. Vui lòng thử lại.";
                await LoadRolesAsync(); // ✅ Reload roles
                return View(model);
            }

            var result = await response.Content.ReadFromJsonAsync<IphoneStoreBE.Common.Models.ResponseResult>();
            if (result?.Success == true)
            {
                TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }

            ViewBag.Error = result?.Message ?? "Đăng ký thất bại.";
            await LoadRolesAsync(); // ✅ Reload roles
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during registration");
            ViewBag.Error = $"Lỗi đăng ký: {ex.Message}";
            await LoadRolesAsync(); // ✅ Reload roles
            return View(model);
        }
    }

    // ✅ Helper method để load Roles từ Backend với DEBUG chi tiết
    private async Task LoadRolesAsync()
    {
        try
        {
            _logger.LogInformation("🔍 Attempting to load roles from API...");

            var response = await _httpClient.GetAsync("Role");
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("📡 Role API Response Status: {StatusCode}", response.StatusCode);
            _logger.LogInformation("📡 Role API Response Content: {Content}", responseContent);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<IphoneStoreBE.Common.Models.ResponseResult<List<RoleViewModel>>>();

                if (result != null && result.Success && result.Data != null)
                {
                    ViewBag.Roles = result.Data;
                    _logger.LogInformation("✅ Loaded {Count} roles successfully", result.Data.Count);

                    // Log từng role để debug
                    foreach (var role in result.Data)
                    {
                        _logger.LogInformation("   - Role: Id={Id}, Name={Name}", role.Id, role.RoleName);
                    }
                }
                else
                {
                    ViewBag.Roles = new List<RoleViewModel>();
                    _logger.LogWarning("⚠️ API returned success but data is null or empty");
                }
            }
            else
            {
                ViewBag.Roles = new List<RoleViewModel>();
                _logger.LogWarning("⚠️ Failed to load roles. Status: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            ViewBag.Roles = new List<RoleViewModel>();
            _logger.LogError(ex, "❌ Error loading roles");
        }
    }

    // Đăng xuất
    [HttpGet("/Account/Logout")]
    public IActionResult Logout() // ✅ Sửa từ IActionAction thành IActionResult
    {
        HttpContext.Session.Clear();
        HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }
}
