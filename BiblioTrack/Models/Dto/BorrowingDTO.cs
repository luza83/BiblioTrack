using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiblioTrack.Models.Dto
{
    public class BorrowingDTO
    {

        public int BorrowId { get; set; }
        public int CopyId { get; set; }
        public BookCopy? Copy { get; set; }
        public Book? Book { get; set; }  
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsOverdue => Status == "Borrowed" && DueDate < DateTime.UtcNow;
    }
}
