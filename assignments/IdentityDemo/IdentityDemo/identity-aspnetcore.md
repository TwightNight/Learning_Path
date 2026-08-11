# Microsoft.AspNetCore.Identity — Khái niệm & Solution demo

## 1. Identity là gì

`Microsoft.AspNetCore.Identity` là hệ thống membership có sẵn của ASP.NET Core: quản lý user,
password, role, claim, external login (Google/Facebook...), 2FA, token reset password/email —
tất cả đóng gói sẵn thay vì tự viết như NovelX đang làm (BCrypt + JWT thủ công).

Các thành phần cốt lõi:

| Thành phần | Vai trò |
|---|---|
| `IdentityUser` | Entity mặc định đại diện 1 user (Id, UserName, Email, PasswordHash, PhoneNumber, LockoutEnd...) |
| `IdentityRole` | Entity đại diện 1 role (Id, Name) |
| `IdentityDbContext<TUser>` | `DbContext` đã định nghĩa sẵn 7 bảng Identity (AspNetUsers, AspNetRoles, AspNetUserRoles...) |
| `UserManager<TUser>` | API tạo/sửa/xóa user, hash password, quản lý role & claim của user |
| `SignInManager<TUser>` | API đăng nhập/đăng xuất, ghi cookie xác thực, xử lý lockout, 2FA |
| `RoleManager<TRole>` | API tạo/sửa/xóa role |
| `PasswordHasher<TUser>` | Hash password bằng PBKDF2 (mặc định), thay thế BCrypt bạn đang dùng |

## 2. IdentityUser — các cột quan trọng

```
Id                     string (GUID dạng chuỗi, khóa chính)
UserName / NormalizedUserName
Email / NormalizedEmail / EmailConfirmed
PasswordHash
PhoneNumber / PhoneNumberConfirmed
TwoFactorEnabled
LockoutEnd / LockoutEnabled / AccessFailedCount
ConcurrencyStamp / SecurityStamp
```

Nếu cần thêm field riêng (ví dụ `DisplayName`, `AvatarUrl` cho NovelX), tạo class kế thừa:

```csharp
public class AppUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}
```

rồi đổi mọi generic type từ `IdentityUser` sang `AppUser` (DbContext, `AddIdentity<AppUser, IdentityRole>()`,
`UserManager<AppUser>`...).

## 3. IdentityDbContext

`IdentityDbContext<TUser>` kế thừa `DbContext` và tự khai báo `OnModelCreating` cho 7 bảng Identity.
Khi bạn tạo `AppDbContext : IdentityDbContext<IdentityUser>`, **bắt buộc** gọi
`base.OnModelCreating(builder)` trước khi cấu hình thêm, nếu không schema Identity sẽ sai.

Nếu NovelX đã có `AppDbContext` riêng (không kế thừa Identity), cách migrate là đổi base class
từ `DbContext` sang `IdentityDbContext<IdentityUser>`, rồi tạo migration mới — EF Core sẽ tự thêm
7 bảng AspNet* vào cạnh các bảng hiện có (Novel, Chapter, Comment...) mà không ảnh hưởng dữ liệu cũ.

## 4. Đăng ký Identity trong Program.cs

```csharp
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => { ... })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
```

`AddIdentity` tự động đăng ký cookie authentication scheme (`IdentityConstants.ApplicationScheme`)
— khác với việc bạn tự cấu hình `AddAuthentication().AddCookie()` hay `AddJwtBearer()` thủ công.
Vẫn cần `ConfigureApplicationCookie()` để chỉnh `LoginPath`, `ExpireTimeSpan`...

Thứ tự middleware **luôn** là:

```csharp
app.UseAuthentication(); // xác định User là ai
app.UseAuthorization();  // xác định User được phép làm gì
```

## 5. So sánh với cách NovelX đang làm (JWT thủ công + BCrypt)

| | NovelX hiện tại | ASP.NET Core Identity |
|---|---|---|
| Hash password | BCrypt tự gọi | `PasswordHasher<TUser>` (PBKDF2), tự động qua `UserManager` |
| Lưu trạng thái đăng nhập | JWT trong HttpOnly cookie, tự parse claims | Cookie xác thực chuẩn của ASP.NET, tự quản lý qua `SignInManager` |
| Role/claim | Tự thêm claim vào JWT khi tạo token | `UserManager.AddToRoleAsync`, `[Authorize(Roles="...")]` có sẵn |
| Lockout, 2FA, reset password | Phải tự viết toàn bộ | Có sẵn API (`AccessFailedCount`, token providers) |
| Linh hoạt / kiểm soát chi tiết | Cao (tự control 100% claims, token) | Thấp hơn, nhưng có thể mở rộng qua `AppUser : IdentityUser` |

**Kết hợp cả hai** hoàn toàn khả thi: dùng Identity để quản lý user/role/password, nhưng khi
đăng nhập thành công thì tự phát hành JWT riêng (thay vì dùng cookie Identity mặc định) —
gọi là "Identity làm store, JWT làm cơ chế xác thực cho API". Đây là hướng phù hợp nếu NovelX
sau này tách frontend/backend hoặc cần API cho mobile.

## 6. Chạy solution demo

```bash
cd IdentityDemo
dotnet restore
dotnet ef migrations add InitIdentity
dotnet ef database update
dotnet run
```

Tài khoản admin được seed sẵn lúc khởi động (xem `Program.cs`):
- Email: `admin@identitydemo.local`
- Mật khẩu: `Admin@123`

Các trang có trong demo: `/Register`, `/Login`, `/Logout` (POST), `/Index` (hiển thị role),
`/Admin` (chỉ role Admin truy cập được, minh họa `[Authorize(Roles = "Admin")]`).
