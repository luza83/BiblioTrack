using BiblioTrack.Models;
using BiblioTrack.Models.Dto;

namespace BiblioTrack.Services
{
    public interface IUserActivityService
    {
        Task<PagedResponse<UserActivityDTO>> GetUsersActivityAsync(GetUserActivityRequest getUserActivityRequest );
        Task<UserActivityDTO> GetUserActivityByIdAsync(string userId, string userName);
    }

}
