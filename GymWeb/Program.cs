using Microsoft.EntityFrameworkCore;
using GymWeb.Models; // Hoặc namespace chứa class DataContext của mày
using GymWeb.Services;
var builder = WebApplication.CreateBuilder(args);

//Thêm dịch vụ Session
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session hết hạn sau 30 phút
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
// Add DB
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null)));

// Gửi email OTP (cấu hình thật nằm trong dotnet user-secrets, xem EmailSettings.cs)
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

builder.Services.AddScoped<GymWeb.Services.BookingConflictService>();
builder.Services.AddScoped<GymWeb.Services.ScheduleConflictService>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

//Seed dữ liệu khi chạy
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DataContext>();
    context.Database.Migrate();// Tự tạo database nếu chưa có
    GymManagement.Data.SeedData.EnsurePopulated(context);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

//Kích hoạt Middleware cho Session (Phải nằm trước app.MapControllerRoute)
app.UseSession();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
