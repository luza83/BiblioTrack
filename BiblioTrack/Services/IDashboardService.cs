using BiblioTrack.Models.Dto;

namespace BiblioTrack.Services
{
    public interface IDashboardService
    {
        Task<DashboardResponseDto> GetDashboardData();

    }
}
