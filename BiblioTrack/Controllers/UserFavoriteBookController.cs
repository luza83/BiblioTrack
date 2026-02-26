using BiblioTrack.Data;
using BiblioTrack.Models;
using BiblioTrack.Models.Dto;
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
        private readonly IWebHostEnvironment _env;
        public UserFavoriteBook(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _response = new ApiResponse();
            _env = env;
        }

        [HttpGet("allFavoriteBooks")]
        public IActionResult GetUserFavoriteBooks()
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("id")?.Value;
            if(currentUserId == null)
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
        [HttpPost("{bookId}", Name = "AddBookToFavorites")]
        public async Task<ActionResult<ApiResponse>> AddBookToFavorites(int bookId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("id")?.Value;
            if (currentUserId == null)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return Ok(_response);
            }

            if (bookId == 0) { 
                _response.IsSuccess =false;
                return BadRequest(_response);
            }
            try
            {
                UserFavoriteBookModel newFavorite = new UserFavoriteBookModel();
                newFavorite.UserId = currentUserId;
                newFavorite.BookId = bookId;

                _db.UserFavoriteBook.Add(newFavorite);
                await _db.SaveChangesAsync();

                _response.StatusCode = HttpStatusCode.Created;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex) {
                _response.IsSuccess = false;
                _response.ErrorMessages
                     = [ex.ToString()];
                return BadRequest(_response);
            }

        }
       
        [HttpDelete("{bookId}", Name = "DeleteFavoriteBook")]
        public async Task<ActionResult<ApiResponse>> DeleteFavoriteBook(int bookId)
        {
            try
            {

                var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("id")?.Value;
                if (currentUserId == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return Ok(_response);
                }
                if (bookId == 0)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                UserFavoriteBookModel? existingFavoriteBook = await _db.UserFavoriteBook.FirstOrDefaultAsync(u => u.UserId == currentUserId && u.BookId == bookId);

                if (existingFavoriteBook == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }


                _db.UserFavoriteBook.Remove(existingFavoriteBook);
                await _db.SaveChangesAsync();

                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;
                return Ok(_response);


            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                     = [ex.ToString()];
                return BadRequest(_response);
            }
           
        }
    }
}
