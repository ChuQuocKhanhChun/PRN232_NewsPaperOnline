using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using PRN232_FinalProject.Models;
using PRN232_FinalProject.Repository.Implement;
using PRN232_FinalProject.Repository.Interfaces;
using PRN232_FinalProject.Services.Implement;
using PRN232_FinalProject.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddOData(opt =>
        opt.Select().Filter().OrderBy().Expand().SetMaxTop(100).Count());
// Đăng ký AutoMapper
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddControllers(
    
    );
// If you're using Razor Views (optional)
builder.Services.AddControllersWithViews();

// Register DbContext
builder.Services.AddDbContext<Prn232FinalProjectContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));

// Register DI for Repository and Service
builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
builder.Services.AddScoped<IArticleService, ArticleService>();
builder.Services.AddSwaggerGen();
// Build app
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers(); // For API controllers including OData

// Optional: Default MVC routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
