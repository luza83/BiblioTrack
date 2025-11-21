using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiblioTrack.Models.Dto
{
    public class BookCopyDTO
    {
 
        [Required]
        public int BookId { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty;

        public string Location { get; set; } = string.Empty;

    }
}
