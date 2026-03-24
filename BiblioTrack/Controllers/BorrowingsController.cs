using BiblioTrack.Data;
using BiblioTrack.Models;
using BiblioTrack.Models.Dto;
using BiblioTrack.Services;
using BiblioTrack.Utility;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;


namespace BiblioTrack.Controllers
{
    [Route("api/borrowings")]
    [ApiController]
    public class BorrowingsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ApiResponse _response;
        private readonly IWebHostEnvironment _env;
        private readonly IBookCopyService _bookCopyService;
        private readonly IBorrowingsService _borrowingsService;
        public BorrowingsController(ApplicationDbContext db, 
                                    IWebHostEnvironment env,
                                    IBookCopyService bookCopyService,
                                    IBorrowingsService borrowingsService)
        {
            _db = db;
            _response = new ApiResponse();
            _env = env;
            _bookCopyService = bookCopyService;
            _borrowingsService = borrowingsService;
        }

        [HttpGet("{userId}", Name = "GetUserBorrowings")]
        public IActionResult GetUserBorrowings(string userId)
        {
            if(string.IsNullOrEmpty(userId))
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = ["Invalid or missing user"];
                return BadRequest(_response);
            }

            _response.Result = _db.Borrowings
                               .Where(x => x.UserId == userId);
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }


        [HttpGet("{borrowId:int}", Name = "GetBorrowingById")]
        public IActionResult GetBorrowingById(int borrowId)
        {
            if (borrowId == 0)
            {
                _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return Ok(_response);
            }
            _response.Result = _db.Borrowings
                               .Where(bc => bc.BorrowId == borrowId);
            _response.StatusCode = System.Net.HttpStatusCode.OK;
            return Ok(_response);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> AddBorrowing([FromBody ] AddBorrowingRequest addBorrowingRequest)
        {
   
            if ( addBorrowingRequest.BookId == 0 || string.IsNullOrEmpty(addBorrowingRequest.UserId))
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages = ["Missing Book Details"];
                return BadRequest(_response);
            }
           
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("id")?.Value;
            var isAdmin = User.IsInRole(SD.Role_Admin);
            var isUserAuthorized = isAdmin || currentUserId == addBorrowingRequest.UserId;
            if (!isUserAuthorized)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return Ok(_response);
            }

            var response = await _borrowingsService.AddBorrowing(addBorrowingRequest);
            if (response is null)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return _response;
            }

            _response.Result = response;
            _response.StatusCode = HttpStatusCode.Created;
            _response.IsSuccess = true;
            return Ok(_response);


        }

        [HttpPut]
        public async Task<ActionResult<ApiResponse>> UpdateBorrowing([FromBody] UpdateBorrowingDTO borrowingUpdateDto)
        {
            if (!ModelState.IsValid)
            {
                _response.IsSuccess = false;
                return BadRequest(_response);
            }
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("id")?.Value;
            var isAdmin = User.IsInRole(SD.Role_Admin);
            if (string.IsNullOrEmpty(currentUserId) || borrowingUpdateDto.BorrowId == 0)
            {
                _response.IsSuccess = false;
                return BadRequest(_response);
            }
            var isUpdated = await _borrowingsService.UpdateBorrowing(currentUserId, borrowingUpdateDto, isAdmin);
            if (!isUpdated)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);


            }
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.NoContent;
            return Ok(_response);


        }

        [HttpDelete("{borrowingId:int}", Name = "DeleteBorrowing")]
        public async Task<ActionResult<ApiResponse>> DeleteBorrowing(int borrowingId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    return BadRequest(_response);

                }

                if (borrowingId == 0)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                Borrowings? existingBorrowing = await _db.Borrowings.FirstOrDefaultAsync(u => u.BorrowId == borrowingId);

                if (existingBorrowing == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                var bookCopyUpdated = await _bookCopyService.UpdateBookCopy(existingBorrowing.CopyId, SD.Book_Copy_Status_Available);

                if (!bookCopyUpdated.Success)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages.Add(bookCopyUpdated.Message);
                    return BadRequest(_response);
                }
                _db.Borrowings.Remove(existingBorrowing);
                await _db.SaveChangesAsync();
                _response.IsSuccess = true;
                _response.StatusCode = HttpStatusCode.NoContent;
                return Ok(_response);


            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                     = [ex.ToString()];
            }

            return BadRequest(_response);
        }

       

    }
}
