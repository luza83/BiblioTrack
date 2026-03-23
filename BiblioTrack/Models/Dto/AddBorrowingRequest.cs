namespace BiblioTrack.Models.Dto
{
    public class AddBorrowingRequest
    {
        public int BookId { get; set; } 
        public string UserId { get; set; } = string.Empty;
    }
}
