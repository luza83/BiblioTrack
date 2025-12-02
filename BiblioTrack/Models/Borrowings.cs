using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;

namespace BiblioTrack.Models
{
    public class Borrowings
    {
        [Key]
        public int BorrowId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = new();

        [Required]
        public int CopyId { get; set; }

        [ForeignKey(nameof(CopyId))]
        public BookCopy? Copy { get; set; }
        [Required]
        public DateTime BorrowDate { get; set; }
        [Required]
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; } 
        public string Status { get; set; } = string.Empty;
    }
}
