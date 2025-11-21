using System.ComponentModel.DataAnnotations;

namespace BiblioTrack.Models.Dto
{
    public class BookCreateDto
    {
    
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Author { get; set; } = string.Empty;
        [Required]
        public string ISBN { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public IFormFile ImageFile { get; set; } = null!;
    }
}
