using BusinessLayers;
using DataAccessTool;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "FoodMart.Shop.Cookie"; // Đặt tên khác với Admin để tránh xung đột
        options.LoginPath = "/User/Login";
        options.AccessDeniedPath = "/User/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });

// Dependency Injection for Business Layer Services
builder.Services.AddScoped<MatHangService>();
builder.Services.AddScoped<KhachHangService>();
builder.Services.AddScoped<GioHangService>();
builder.Services.AddScoped<DonHangService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

var adminImagesPath = Path.Combine(builder.Environment.ContentRootPath, "..", "SV22T1020239.Admin", "wwwroot", "images");
if (Directory.Exists(adminImagesPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(adminImagesPath),
        RequestPath = "/images" // Khi web gọi /images/... nó sẽ tìm thêm ở folder Admin
    });
}

app.UseSession();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
