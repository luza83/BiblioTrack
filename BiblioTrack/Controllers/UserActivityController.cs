using BiblioTrack.Data;
using BiblioTrack.Models;
using BiblioTrack.Models.Dto;
using BiblioTrack.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace BiblioTrack.Controllers
{
    [Route("api/userActivity")]
    [ApiController]
    public class UserActivityController : Controller
    {
        private readonly ApiResponse _response;
        private readonly IUserActivityService _userActivityService;
        public UserActivityController(IUserActivityService userActivityService)
        {
            _response = new ApiResponse();
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

        [HttpGet("userOverview")]
        public async Task<IActionResult> GetUserBooksOverview()
        {
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("id")?.Value;
            var currentUserName = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("username")?.Value ?? "";

            if (string.IsNullOrEmpty(currentUserId))
            {
                _response.IsSuccess = false;
                _response.StatusCode = System.Net.HttpStatusCode.Unauthorized;
                _response.ErrorMessages = ["Unauthorized access"];
                return Unauthorized(_response);
            }

            var userBookOverview =  await _userActivityService.GetUserActivityByIdAsync(currentUserId,currentUserName);

            if (userBookOverview == null)
            {
                _response.IsSuccess = false;
                _response.StatusCode = System.Net.HttpStatusCode.NotFound;
                _response.ErrorMessages = ["User book overview not found"];
                return NotFound(_response);
            }

            _response.IsSuccess = true;
            _response.StatusCode = System.Net.HttpStatusCode.OK;
            _response.Result = userBookOverview;
            return Ok(_response);
        }
    }
}
