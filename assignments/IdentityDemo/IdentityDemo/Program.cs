using IdentityDemo.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. EF Core + SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Đăng ký Identity: IdentityUser (user) + IdentityRole (role, dùng string Id)
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Cấu hình chính sách mật khẩu (mặc định khá chặt, chỉnh lại cho phù hợp demo)
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;

    // Khóa tài khoản sau nhiều lần đăng nhập sai
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);

    // Bắt buộc email duy nhất (UserName mặc định = Email trong demo này)
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<AppDbContext>() // Identity lưu dữ liệu qua AppDbContext
    .AddDefaultTokenProviders();              // cần cho reset password, email confirmation token...

// 3. Cấu hình Cookie mà Identity dùng để đăng nhập (khác với cookie JWT thủ công bên NovelX)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Login";
    options.LogoutPath = "/Logout";
    options.AccessDeniedPath = "/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true; // giống nguyên tắc HttpOnly Doc đang dùng cho JWT ở NovelX
});

builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication(); // PHẢI đứng trước UseAuthorization
app.UseAuthorization();

app.MapRazorPages();

// 4. Seed role "Admin" và 1 tài khoản admin mặc định khi ứng dụng khởi động (chỉ để demo)
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    const string adminRole = "Admin";
    if (!await roleManager.RoleExistsAsync(adminRole))
    {
        await roleManager.CreateAsync(new IdentityRole(adminRole));
    }

    const string adminEmail = "admin@identitydemo.local";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser is null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };
        // UserManager tự hash mật khẩu bằng PasswordHasher<TUser> (PBKDF2), không cần BCrypt thủ công
        var result = await userManager.CreateAsync(adminUser, "Admin@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, adminRole);
        }
    }
}

app.Run();
