using haru_community.Data;
using haru_community.Models;
using haru_community.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// MVC 서비스 추가
builder.Services.AddControllersWithViews();


// 데이터베이스 컨텍스트 설정
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 인증 및 권한 부여 설정 identity
builder.Services
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 6;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.Configure<DefaultAdminOptions>(builder.Configuration.GetSection("DefaultAdmin"));

// Razor Pages 서비스 추가
builder.Services.AddRazorPages();

var app = builder.Build();

await AdminSeeder.SeedAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 인증 및 인가 순서 중요
app.UseAuthentication();
app.UseAuthorization();

// MVC 라우팅 설정
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// identity UI 라우트 설정
app.MapRazorPages();

app.Run();
