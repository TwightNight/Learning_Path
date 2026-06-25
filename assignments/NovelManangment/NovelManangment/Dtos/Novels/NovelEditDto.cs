using NovelManangment.Models;
using System.ComponentModel.DataAnnotations;

namespace NovelManangment.Dtos.Novels
{
    public class NovelEditDto
    {
        public int NovelId { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(255)]
        public string Title { get; set; } = null!;

        [StringLength(255)]
        public string? AlternativeTitle { get; set; }

        [Required(ErrorMessage = "Author name is required.")]
        [StringLength(100)]
        public string AuthorName { get; set; } = null!;

        [StringLength(100)]
        public string? ArtistName { get; set; }

        public NovelType Type { get; set; }
        public NovelStatus Status { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = null!;

        [Required(ErrorMessage = "Language is required.")]
        [StringLength(100, ErrorMessage = "Language cannot exceed 100 characters.")]
        public string Language { get; set; } = null!;

        [Url(ErrorMessage = "Cover URL must be a valid URL.")]
        public string? CoverUrl { get; set; }

        public List<int> GenreIds { get; set; } = new();
    }
}
