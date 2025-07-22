using Microsoft.AspNetCore.Authentication.Cookies;
using PRN232_FinalProject_Client.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient<PRN232_FinalProject_Client.Services.ArticleService>();
builder.Services.AddHttpClient<PRN232_FinalProject_Client.Services.AuthService>();
builder.Services.AddHttpClient<PRN232_FinalProject_Client.Services.UserService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<UserService>();
// Thêm xác thực cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth2/Login"; // Đường dẫn khi chưa đăng nhập
        options.AccessDeniedPath = "/Auth2/AccessDenied"; // Đường dẫn khi không có quyền
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Yêu cầu HTTPS
        options.Cookie.SameSite = SameSiteMode.Strict; // Bảo mật SameSite
    });
builder.Services.AddSession();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
