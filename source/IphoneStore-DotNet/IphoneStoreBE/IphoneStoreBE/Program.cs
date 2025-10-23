using IphoneStoreBE.Context;
using IphoneStoreBE.Services;
using IphoneStoreBE.Services.IServices;
using IphoneStoreBE.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Session;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ===================================================
// 1️⃣ Controllers & Swagger
// ===================================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "iPhone Store API", 
        Version = "v1" 
    });

    // Cấu hình để sử dụng lowercase URLs
    c.UseInlineDefinitionsForEnums();

    // ✅ Cấu hình chuẩn cho JWT Bearer: Swagger sẽ tự thêm tiền tố "Bearer "
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhập JWT token (không cần gõ từ 'Bearer').",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// ===================================================
// 2️⃣ Database Context
// ===================================================
builder.Services.AddDbContext<IphoneStoreContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null)));

// ===================================================
// 3️⃣ Dependency Injection (Services)
// ===================================================
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddHttpContextAccessor();

// ===================================================
// 4️⃣ Session (Cấu hình Session cho Backend)
// ===================================================
builder.Services.AddDistributedMemoryCache(); // Cấu hình bộ nhớ cache để lưu trữ session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);  // Thiết lập thời gian timeout của session
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".IphoneStoreBE.Session"; // Đặt tên cho cookie session
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Sử dụng cookie bảo mật cho HTTPS
});

// ===================================================
// 5️⃣ CORS cho Frontend (Cấu hình CORS để cho phép frontend giao tiếp với backend)
// ===================================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
              "https://localhost:7223",  // ✅ Frontend port
              "http://localhost:7223"    // ✅ Also allow HTTP if needed
          )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Cho phép gửi cookie và session
    });
});

// ===================================================
// 6️⃣ JWT Authentication (Cấu hình JWT Authentication cho các API cần bảo mật)
// ===================================================
builder.Services.AddAuthentication(options =>
{
    // ✅ API mặc định xác thực bằng JWT (tránh redirect sang /api/auth/login gây 405)
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/api/auth/login";
    options.LogoutPath = "/api/auth/logout";
    options.AccessDeniedPath = "/api/auth/accessdenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.SlidingExpiration = true;
    options.Cookie.Name = ".IphoneStore.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    // ✅ Không redirect trong API, trả về 401/403 thay vì chuyển hướng
    options.Events = new CookieAuthenticationEvents
    {
        OnRedirectToLogin = ctx => { ctx.Response.StatusCode = StatusCodes.Status401Unauthorized; return Task.CompletedTask; },
        OnRedirectToAccessDenied = ctx => { ctx.Response.StatusCode = StatusCodes.Status403Forbidden; return Task.CompletedTask; }
    };
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        ValidIssuer = "IphoneStoreBackend",
        ValidAudience = "IphoneStoreFrontend",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("SuperSecretKeyForJwtTokenGeneration12345678"))
    };
});

// ===================================================
// 7️⃣ Authorization Policies
// ===================================================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOrAdmin", policy => policy.RequireRole("User", "Admin"));
});

// ===================================================
// 8️⃣ Cấu hình CORS trước khi sử dụng Session và Authentication
// ===================================================
var app = builder.Build();

// ===================================================
// 8️⃣ Middleware Pipeline (THỨ TỰ QUAN TRỌNG)
// ===================================================
app.UseHttpsRedirection();

// ✅ Enable static file serving from wwwroot
app.UseStaticFiles();

app.UseCors("AllowFrontend");  // ✅ Bật CORS trước Session
app.UseSession();              // ✅ Phải có session

// ✅ Bắt buộc lowercase PATH trước khi routing để tránh 405 Method Not Allowed
app.Use(async (context, next) =>
{
    context.Request.Path = context.Request.Path.Value?.ToLowerInvariant();
    await next();
});

// ✅ Thiết lập routing sau khi đã chuẩn hóa đường dẫn
app.UseRouting();

app.UseAuthentication();       // ✅ Đọc token JWT
app.UseAuthorization();        // ✅ Xác thực quyền người dùng

// ✅ Thêm logging để debug
app.Use(async (context, next) =>
{
    Console.WriteLine($"🔍 Request: {context.Request.Method} {context.Request.Path}");
    Console.WriteLine($"🔍 Authorization Header: {context.Request.Headers.Authorization}");
    await next();
});

// ✅ Map controllers sau khi đã cấu hình authentication
app.MapControllers();

// ===================================================
// 9️⃣ Swagger (DEV Only)
// ===================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ===================================================
// 10️⃣ Chạy app
// ===================================================

// ✅ Tự động tạo database và seed data
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<IphoneStoreContext>();
        
        // Tự động tạo database nếu chưa tồn tại
        Console.WriteLine("🔄 Ensuring database exists...");
        await context.Database.EnsureCreatedAsync();
        Console.WriteLine("✅ Database ensured!");
        
        // Kiểm tra và tạo roles mặc định
        if (!context.Roles.Any())
        {
            context.Roles.AddRange(
                new Role { RoleName = "Admin" },
                new Role { RoleName = "User" }
            );
            await context.SaveChangesAsync();
            Console.WriteLine("✅ Created default roles: Admin, User");
        }
        else
        {
            Console.WriteLine("✅ Default roles already exist");
        }
        
        // Kiểm tra và tạo categories mặc định
        if (!context.Categories.Any())
        {
            context.Categories.AddRange(
                new Category { CategoryName = "iPhone", IsActive = true, CreatedDate = DateTime.UtcNow },
                new Category { CategoryName = "MacBook", IsActive = true, CreatedDate = DateTime.UtcNow },
                new Category { CategoryName = "iPad", IsActive = true, CreatedDate = DateTime.UtcNow },
                new Category { CategoryName = "Apple Watch", IsActive = true, CreatedDate = DateTime.UtcNow },
                new Category { CategoryName = "AirPods", IsActive = true, CreatedDate = DateTime.UtcNow }
            );
            await context.SaveChangesAsync();
            Console.WriteLine("✅ Created default categories");
        }
        else
        {
            Console.WriteLine("✅ Default categories already exist");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error setting up database: {ex.Message}");
        Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
        Console.WriteLine("⚠️  Application will continue without seeding data");
    }
}

app.Run();
