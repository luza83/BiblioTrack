using System.ComponentModel.DataAnnotations;

namespace BiblioTrack.Models.Dto
{
    public class BorrowableBookDto :Book
    {
        public int TotalCopies { get; set; }
        public bool? IsUserFavorite { get; set; } 

    }
}
