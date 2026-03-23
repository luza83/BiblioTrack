using System.ComponentModel.DataAnnotations;

namespace BiblioTrack.Models.Dto
{
    public class UpdateBorrowingDTO
    {
        [Required]
        public int BorrowId { get; set; }
        public DateTime DueDate { get; set; }
        public string NewBorrowStatus { get; set; } = string.Empty;
    }
}
