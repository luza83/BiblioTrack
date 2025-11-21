using System.ComponentModel.DataAnnotations;

namespace BiblioTrack.Models.Dto
{
    public class UpdateBorrowingDTO
    {
        [Required]
        public int BorrowId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int CopyId { get; set; }

        [Required]
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
