using System.ComponentModel.DataAnnotations;

namespace BiblioTrack.Models.Dto
{
    public class BookAndCopiesDTO
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Publisher { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int TotalCopies { get; set; }
    }
}
