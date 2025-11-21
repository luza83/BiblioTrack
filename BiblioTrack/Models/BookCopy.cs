
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiblioTrack.Models
{
    public class BookCopy
    {
        [Key]
        public int CopyId { get; set; }
        [Required]
        public int BookId { get; set; }

        [ForeignKey("BookId")]
        public Book? Book { get; set; }
        [Required]
        public string Status { get; set; } = string.Empty;
 
        public string Location { get; set; } = string.Empty;

    }
}
