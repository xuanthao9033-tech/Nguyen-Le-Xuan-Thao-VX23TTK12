using IphoneStoreFE.Services;
using IphoneStoreFE.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// ===================================================
// 1️⃣ MVC Controllers
// ===================================================
builder.Services.AddControllersWithViews();

// ===================================================
// 2️⃣ HttpContext Accessor (must be registered before HttpClient)
// ===================================================
builder.Services.AddHttpContextAccessor();

// ===================================================
// 3️⃣ Auth Header Handler
// ===================================================
builder.Services.AddTransient<AuthHeaderHandler>();

// ===================================================
// 4️⃣ HttpClient (gọi API Backend)
// ===================================================
builder.Services.AddHttpClient();

// ✅ Đăng ký ProductService
builder.Services.AddScoped<ProductService>();
builder.Services.AddHttpClient<ProductService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7182/api/"); // 🔗 Port Backend
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<AuthHeaderHandler>(); // ✅ Add auth handler

// ✅ Đăng ký CartService (typed client)
builder.Services.AddHttpClient<ICartService, CartService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7182/api/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<AuthHeaderHandler>(); // ✅ Add auth handler

// ✅ Đăng ký OrderService (typed client)
builder.Services.AddHttpClient<IOrderService, OrderService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7182/api/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<AuthHeaderHandler>(); // ✅ Add auth handler

// ✅ HttpClient cho các controller khác (AdminController, etc.)
builder.Services.AddHttpClient("IphoneStoreAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7182/api/"); // ✅ Fixed: Added /api/
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddHttpMessageHandler<AuthHeaderHandler>(); // ✅ Add auth handler

// ===================================================
// 5️⃣ Session (ép luôn cross-site cookie)
// ===================================================
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2); // Session timeout sau 2 giờ
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".IphoneStore.Session";

    // ⚙️ Ép cookie cross-port (FE 7223 ↔ BE 7182)
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// ===================================================
// 6️⃣ Authentication (Cookie-based cho [Authorize])
// ===================================================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";        // Trang đăng nhập
        options.LogoutPath = "/Account/Logout";      // Trang đăng xuất
        options.AccessDeniedPath = "/Account/AccessDenied";  // Trang từ chối quyền truy cập
        options.ExpireTimeSpan = TimeSpan.FromHours(2);  // Session sẽ hết hạn sau 2 giờ
        options.SlidingExpiration = true;  // Hết hạn động (tự động gia hạn thời gian)

        // Cấu hình cookie
        options.Cookie.Name = ".IphoneStore.Auth"; // Tên cookie
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

// ===================================================
// 7️⃣ Add Razor Pages (nếu có)
// ===================================================
builder.Services.AddRazorPages();

// ===================================================
// 8️⃣ Build App
// ===================================================
var app = builder.Build();

// ===================================================
// 9️⃣ Middleware Pipeline (thứ tự quan trọng)
// ===================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Session phải được gọi trước Authentication
app.UseSession();
app.UseAuthentication();  // Xử lý đăng nhập
app.UseAuthorization();   // Xử lý phân quyền

// ===================================================
// 10️⃣ Default Route
// ===================================================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ===================================================
// 11️⃣ Run App
// ===================================================
app.Run();
