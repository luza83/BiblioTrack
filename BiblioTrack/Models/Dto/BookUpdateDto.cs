using System.ComponentModel.DataAnnotations;

namespace BiblioTrack.Models.Dto
{
    public class BookUpdateDto
    {
        [Key]
        public int BookId { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; } 
        public string? ISBN { get; set; } 
        public string? Publisher { get; set; } 
        public string? Category { get; set; } 
        public DateTime CreatedAt { get; set; }
        public string? ImageUrl { get; set; }
    }
}
