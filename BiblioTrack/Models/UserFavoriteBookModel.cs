using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiblioTrack.Models
{
    public class UserFavoriteBookModel
    {
        public int Id { get; set; }   

        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        public int BookId { get; set; }
        public Book? Book { get; set; }
    }
}
