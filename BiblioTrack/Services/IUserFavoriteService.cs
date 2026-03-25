using BiblioTrack.Models.Dto;

namespace BiblioTrack.Services
{
    public interface IUserFavoriteService
    {
        //Task<List<UserFavoriteDto>> GetUserFavorites(string userId);
        Task<bool> AddToFavorites(UserFavoriteBooksRequest userFavoriteBooksRequest);
        Task<bool> RemoveFromFavorites(UserFavoriteBooksRequest userFavoriteBooksRequest);
    }
}
