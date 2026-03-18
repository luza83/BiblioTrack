using BiblioTrack.Models.Dto;

namespace BiblioTrack.Services
{
    public interface IUserActivityService
    {
        Task<PagedResponse<UserActivityDTO>> GetUsersActivityAsync(GetUserActivityRequest getUserActivityRequest );
    }

}
