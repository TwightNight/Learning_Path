using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityDemo.Data;

// IdentityDbContext<IdentityUser> đã tự động khai báo các DbSet cần thiết:
// AspNetUsers, AspNetRoles, AspNetUserRoles, AspNetUserClaims,
// AspNetUserLogins, AspNetUserTokens, AspNetRoleClaims.
// Nếu chỉ cần Role dạng string (Guid Id mặc định) thì kế thừa IdentityDbContext<IdentityUser>
// là đủ, không cần generic IdentityRole riêng vì nó đã bao gồm sẵn.
public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Nếu sau này NovelX cần thêm bảng riêng (Novel, Chapter, Comment...),
    // khai báo DbSet ở đây và cấu hình quan hệ FK tới IdentityUser.Id (string, kiểu GUID dạng chuỗi).
    // public DbSet<Novel> Novels => Set<Novel>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // BẮT BUỘC gọi trước, để Identity tạo đúng schema các bảng AspNet*
    }
}
