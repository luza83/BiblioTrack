namespace BiblioTrack.Models.Dto
{
    public class UserFavoriteDto
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public Book? Book { get; set; }
        public bool IsBorrowable { get; set; } = false;
    }
}
