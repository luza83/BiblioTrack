namespace BiblioTrack.Models.Dto
{
    public class UserFavoriteBooksRequest
    {
        public string UserId { get; set; } = string.Empty;
        public int BookId { get; set; } = 0;
        
    }
}
