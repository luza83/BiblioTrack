using BiblioTrack.Models;
using BiblioTrack.Services;
using Microsoft.AspNetCore.Mvc;

namespace BiblioTrack.Controllers
{
    
    [Route("api/dashboard")]
    [ApiController]
    public class DashboardController : Controller
    {
        private readonly ApiResponse _response;
        private readonly IDashboardService _dashboardService;
        public DashboardController(IDashboardService dashboardService)
        {
            _response = new ApiResponse();
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse>> GetDashboardData()
        {

            var result = await _dashboardService.GetDashboardData();
            if (result == null) {
                _response.IsSuccess = false;
                _response.ErrorMessages = ["Failed to fetch dashboard data"];
                _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                return _response;
            }
            _response.IsSuccess = true;
            _response.Result = result;
            _response.StatusCode = System.Net.HttpStatusCode.OK;
            return _response;
        }
    }
}
