using GrpcMicroservice.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PRN232_FinalProject.Identity;
using PRN232_FinalProject.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
// Register DbContext
builder.Services.AddDbContext<Prn232FinalProjectContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MyCnn")));
// Thêm Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<Prn232FinalProjectContext>()
    .AddDefaultTokenProviders();
var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<ArticleService>();
app.MapGrpcService<CommentService>();
app.MapGrpcService<AccountService>();
app.MapGrpcService<TagGrpcService>();
app.MapGrpcService<CategoryGrpcService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
