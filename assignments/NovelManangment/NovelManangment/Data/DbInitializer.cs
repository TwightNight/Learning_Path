using NovelManangment.Models;
using Microsoft.EntityFrameworkCore;

namespace NovelManangment.Data
{
    public static class DbInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = serviceProvider.GetRequiredService<ApplicationDbContext>())
            {
                // Kiểm tra nếu đã có dữ liệu thì không seed nữa
                if (context.Users.Any()) return;

                // 1. Seed Genres (Thể loại)
                var action = new Genre { Name = "Action", Slug = "action", Description = "Hành động kịch tính" };
                var fantasy = new Genre { Name = "Fantasy", Slug = "fantasy", Description = "Thế giới huyền ảo" };
                var romance = new Genre { Name = "Romance", Slug = "romance", Description = "Tình cảm lãng mạn" };
                var sliceOfLife = new Genre { Name = "Slice of Life", Slug = "slice-of-life", Description = "Đời thường" };

                context.Genres.AddRange(action, fantasy, romance, sliceOfLife);

                // 2. Seed Users
                var admin = new User
                {
                    UserName = "admin",
                    Email = "admin@novel.com",
                    Password = "hashed_password_here", // Thực tế nên hash password
                    DisplayName = "Tổng Quản",
                    Role = UserRole.Admin
                };

                var publisher = new User
                {
                    UserName = "hako_pub",
                    Email = "pub@novel.com",
                    Password = "hashed_password_here",
                    DisplayName = "Nhóm Dịch Hako",
                    Role = UserRole.Publisher
                };

                context.Users.AddRange(admin, publisher);
                context.SaveChanges(); // Lưu để lấy ID cho các bảng sau

                // 3. Seed Novel
                var novel = new Novel
                {
                    PublisherId = publisher.Id,
                    Title = "Thất Nghiệp Chuyển Sinh",
                    AlternativeTitle = "Mushoku Tensei",
                    Slug = "that-nghiep-chuyen-sinh",
                    Description = "Một thanh niên thất nghiệp được chuyển sinh sang thế giới khác...",
                    AuthorName = "Rifujin na Magonote",
                    ArtistName = "Shirotaka",
                    Type = NovelType.HumanTranslated,
                    Status = NovelStatus.Ongoing,
                    CoverUrl = "/uploads/covers/mushoku-tensei.jpg",
                    Genres = new List<Genre> { action, fantasy } // Gán thể loại (Many-to-Many)
                };

                context.Novels.Add(novel);
                context.SaveChanges();

                // 4. Seed Volumes
                var vol1 = new Volume
                {
                    NovelId = novel.Id,
                    Title = "Tập 1: Thời thơ ấu",
                    VolumeNumber = 1,
                    Description = "Cuộc sống mới của Rudeus tại làng Bueno",
                    Type = VolumeType.Normal,
                    CoverUrl = "/uploads/volumes/vol1.jpg",
                    PdfUrl = "/uploads/pdfs/vol1.pdf"
                };

                var vol2 = new Volume
                {
                    NovelId = novel.Id,
                    Title = "Tập 2: Gia sư tại Roa",
                    VolumeNumber = 2,
                    Description = "Rudeus đi làm gia sư cho tiểu thư Eris",
                    Type = VolumeType.Normal,
                    CoverUrl = "/uploads/volumes/vol2.jpg"
                };

                context.Volumes.AddRange(vol1, vol2);
                context.SaveChanges();

                // 5. Seed Chapters
                context.Chapters.AddRange(
                    new Chapter
                    {
                        NovelId = novel.Id,
                        VolumeId = vol1.Id,
                        Title = "Chương 01: Thế giới mới",
                        Slug = "chuong-01-the-gioi-moi",
                        ChapterNumber = 1,
                        Content = "<p>Nội dung chương 1...</p><img src='/uploads/images/img1.jpg'/>",
                        WordCount = 1200,
                        ViewCount = 150
                    },
                    new Chapter
                    {
                        NovelId = novel.Id,
                        VolumeId = vol1.Id,
                        Title = "Chương 02: Ma thuật sư",
                        Slug = "chuong-02-ma-thuat-su",
                        ChapterNumber = 2,
                        Content = "<p>Nội dung chương 2...</p>",
                        WordCount = 1500,
                        ViewCount = 120
                    },
                    new Chapter
                    {
                        NovelId = novel.Id,
                        VolumeId = vol2.Id,
                        Title = "Chương 01: Gặp gỡ Eris",
                        Slug = "chuong-01-gap-go-eris",
                        ChapterNumber = 1,
                        Content = "<p>Nội dung chương 1 tập 2...</p>",
                        WordCount = 2000,
                        ViewCount = 90
                    }
                );

                context.SaveChanges();
            }
        }
    }
}