using Microsoft.EntityFrameworkCore;
using TestThuVien.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình chuỗi kết nối database trực tiếp từ file context hoặc appsettings.json
// (Chuỗi kết nối lấy theo thông số cấu hình mặc định trong file QuanLyThuVienContext.cs của bạn)
var connectionString = "Server=localhost;Database=QuanLyThuVien;User Id=sa;Password=12345;TrustServerCertificate=True;";

builder.Services.AddDbContext<QuanLyThuVienContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Đăng ký dịch vụ Controller với View (ASP.NET Core MVC)
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 3. Cấu hình luồng xử lý Request Pipeline (Middleware)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Cho phép tải các file giao diện tĩnh (CSS, JS, hình ảnh) trong thư mục wwwroot
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// 4. Định tuyến mặc định dẫn thẳng vào trang Dashboard (Home/Index)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();