using BiblioTrack.Data;
using BiblioTrack.Models;
using BiblioTrack.Models.Dto;
using BiblioTrack.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace BiblioTrack.Controllers
{
    [Route("api/userActivity")]
    [ApiController]
    public class UserActivityController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ApiResponse _response;
        private readonly IWebHostEnvironment _env;
        private readonly IUserActivityService _userActivityService;
        public UserActivityController(ApplicationDbContext db, IWebHostEnvironment env, IUserActivityService userActivityService)
        {
            _db = db;
            _response = new ApiResponse();
            _env = env;
            _userActivityService = userActivityService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsersAsync([FromQuery] GetUserActivityRequest getUserActivityRequest)
        {
            var response = new ApiResponse();

            try
            {
                var users = await _userActivityService.GetUsersActivityAsync(getUserActivityRequest);

                response.StatusCode = HttpStatusCode.OK;
                response.IsSuccess = true;
                response.Result = users;

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.StatusCode = HttpStatusCode.InternalServerError;
                response.IsSuccess = false;
                response.ErrorMessages.Add(ex.Message);

                return StatusCode(500, response);
            }
        }


    }
}
