using BiblioTrack.Models;

namespace BiblioTrack.Services
{
    public interface IUserActivityService
    {
        Task<List<UserActivityModel>> GetUsersActivityAsync();
    }

}
