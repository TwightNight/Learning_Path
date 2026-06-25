using System.ComponentModel.DataAnnotations;

namespace NovelManangment.Dtos.Genres
{

    public class GenreUpsertDto
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(50, ErrorMessage = "Max 50 characters.")]
        public string Name { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Max 500 characters.")]
        public string? Description { get; set; }
    }
}
