namespace BiblioTrack.Models.Dto
{
    public class DashboardResponseDto
    {
        public int BookCount { get; set; }
        public int FavoriteBookCount { get; set; }
        public int ReservedBookCount { get; set; }
        public int BorrowedBookCount { get; set; }
        public BorrowableBookDto BookOfTheDay { get; set; } = new BorrowableBookDto();
        public List<BorrowableBookDto> TrendingBooks { get; set; }  = new List<BorrowableBookDto>();
        public List<BorrowableBookDto> NewBooks { get; set; } = new List<BorrowableBookDto>();

    }
}
