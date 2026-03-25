using BiblioTrack.Data;
using BiblioTrack.Models;
using BiblioTrack.Models.Dto;
using BiblioTrack.Services;
using BiblioTrack.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;

namespace BiblioTrack.Controllers
{
    [Route("api/userFavoriteBook")]
    [ApiController]
    public class UserFavoriteBook : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ApiResponse _response;
        private readonly IUserFavoriteService _userFavoriteService;
        private readonly IWebHostEnvironment _env;
        public UserFavoriteBook(ApplicationDbContext db, IWebHostEnvironment env, IUserFavoriteService userFavoriteService)
        {
            _db = db;
            _response = new ApiResponse();
            _userFavoriteService = userFavoriteService;
            _env = env;
        }

        [HttpGet("allFavoriteBooks")]
        public IActionResult GetUserFavoriteBooks(UserFavoriteBooksRequest userFavoriteBooksRequest)
        {
            //TODO  unused endpoint- bug fix and refactore needed if used
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("id")?.Value;
            var isAdmin = User.IsInRole(SD.Role_Admin);
     
            if (currentUserId == null || currentUserId != userFavoriteBooksRequest.UserId || !isAdmin)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return Ok(_response);
            }

            _response.Result = _db.UserFavoriteBook.Where(u=> u.UserId == currentUserId).ToList();
            _response.IsSuccess = true;
            _response.StatusCode = System.Net.HttpStatusCode.OK;
            return Ok(_response);
        }
        [HttpPost]
        public async Task<ActionResult<ApiResponse>> AddBookToFavorites([FromBody] UserFavoriteBooksRequest userFavoriteBooksRequest)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("id")?.Value;
            var isAdmin = User.IsInRole(SD.Role_Admin);
            var isAuthorized = (currentUserId != userFavoriteBooksRequest.UserId) || !isAdmin;
            if (currentUserId == null || 
                userFavoriteBooksRequest.BookId == 0 ||
                !isAuthorized)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return Ok(_response);
            }

            var result = await _userFavoriteService.AddToFavorites(userFavoriteBooksRequest);

            if (!result) { 
                _response.IsSuccess =false;
                return BadRequest(_response);
            }

            _response.StatusCode = HttpStatusCode.Created;
            _response.IsSuccess = true;
            return Ok(_response);

        }
       
        [HttpDelete]
        public async Task<ActionResult<ApiResponse>> DeleteFavoriteBook([FromBody] UserFavoriteBooksRequest userFavoriteBooksRequest)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("id")?.Value;
            var isAdmin = User.IsInRole(SD.Role_Admin);
            var isAuthorized = (currentUserId != userFavoriteBooksRequest.UserId) || !isAdmin;
            if (currentUserId == null ||
                userFavoriteBooksRequest.BookId == 0 ||
                !isAuthorized)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return Ok(_response);
            }

            var result = await _userFavoriteService.RemoveFromFavorites(userFavoriteBooksRequest);

            if (!result)
            {
                _response.IsSuccess = false;
                return BadRequest(_response);
            }

            _response.StatusCode = HttpStatusCode.NoContent;
            _response.IsSuccess = true;
            return Ok(_response);

        }
    }
}
