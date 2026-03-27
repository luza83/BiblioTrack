namespace BiblioTrack.Models.Dto
{
    public class UserActivityDTO
    {
        public string? UserId { get; set; } = string.Empty;
        public string? UserName { get; set; } = string.Empty;
        public List<BorrowingDTO> BorrowedBooks { get; set; } = new List<BorrowingDTO>();
        public List<BorrowingDTO> ReservedBooks { get; set; } = new List<BorrowingDTO>();
        public List<UserFavoriteDto> FavoriteBooks { get; set; } = new List<UserFavoriteDto>();
    }
}
