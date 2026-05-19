namespace BiblioTrack.Models.Dto
{
    public class RateBookDto
    {
        public string UserId { get; set; } = string.Empty;
        public int BookId { get; set; }
        public int Rating { get; set; }
    }
}
