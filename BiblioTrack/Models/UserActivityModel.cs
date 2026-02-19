namespace BiblioTrack.Models
{
    public class UserActivityModel
    {
        public string? UserId { get; set; } = string.Empty;
        public string? UserName { get; set; } = string.Empty;
        public List<BookCopy> BorrowedBooks { get; set; } = new List<BookCopy>();
        public List<BookCopy> ReservedBooks { get; set; } = new List<BookCopy>();
        public List<BookCopy> OverdueBooks { get; set; } = new List<BookCopy>();
        public List<BookCopy> FavoriteBooks { get; set; } = new List<BookCopy>();
    }
}
