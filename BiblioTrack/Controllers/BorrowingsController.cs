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
        private readonly BookCopyService _bookCopyService;
        public BorrowingsController(ApplicationDbContext db, 
                                    IWebHostEnvironment env,
                                    BookCopyService bookCopyService)
        {
            _db = db;
            _response = new ApiResponse();
            _env = env;
            _bookCopyService = bookCopyService;
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

        [HttpPost("{bookId}", Name = "AddBorrowing")]
        public async Task<ActionResult<ApiResponse>> AddBorrowing(int bookId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("id")?.Value;
            if (currentUserId == null)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return Ok(_response);
            }
   

            if ( bookId == 0)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages = ["Missing Book Details"];
                return BadRequest(_response);
            }

            try
            {
                var firstAvailableCopy = _db.BookCopy
                                .Where(bc => bookId == bc.BookId && bc.Status == SD.Book_Copy_Status_Available)
                                .FirstOrDefault();

                if (firstAvailableCopy == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages.Add("No book copies available for this book");
                    return BadRequest(_response);
                }
                Borrowings borrowing = new()
                {
                    UserId = currentUserId,
                    CopyId = firstAvailableCopy.CopyId,
                    BorrowDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(15),
                    Status = SD.Borrowing_Status_Borrowed
                };

                var bookCopyUpdated = await _bookCopyService.UpdateBookCopy(firstAvailableCopy.CopyId,
                                                                                    SD.Book_Copy_Status_Borrowed);

                if (!bookCopyUpdated.Success)
                {

                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.ErrorMessages.Add(bookCopyUpdated.Message);
                    return BadRequest(_response);
                }
                _db.Borrowings.Add(borrowing);
                await _db.SaveChangesAsync();
                _response.StatusCode = HttpStatusCode.Created;
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

        [HttpPut("{borrowId:int}", Name = "UpdateBorrowing")]
        public async Task<ActionResult<ApiResponse>> UpdateBorrowing(int borrowId, [FromForm] UpdateBorrowingDTO borrowingUpdateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    return BadRequest(_response);
                }

                Borrowings? existingBorrowing = await _db.Borrowings.FindAsync(borrowId);


                if (existingBorrowing == null || 
                    borrowingUpdateDto == null || 
                    existingBorrowing?.BorrowId != borrowId || 
                    !string.Equals(existingBorrowing.UserId, borrowingUpdateDto.UserId))
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                bool updateBookCopy = false;

                if (existingBorrowing.Status == SD.Borrowing_Status_Borrowed &&
                    borrowingUpdateDto.DueDate != DateTime.MinValue && 
                    existingBorrowing.DueDate < borrowingUpdateDto.DueDate)
                {
                    existingBorrowing.DueDate = borrowingUpdateDto.DueDate;
                    existingBorrowing.Status = SD.Borrowing_Status_Borrowed;
                }

                if (borrowingUpdateDto.Status.Length != 0 && existingBorrowing.Status != borrowingUpdateDto.Status && borrowingUpdateDto.Status == SD.Borrowing_Status_Returned)
                {
                    existingBorrowing.Status = borrowingUpdateDto.Status;
                    existingBorrowing.ReturnDate = DateTime.Now;
                    updateBookCopy = true;
                }

                if (updateBookCopy)
                {
                    var copyUpdated = await _bookCopyService.UpdateBookCopy(existingBorrowing.CopyId, SD.Book_Copy_Status_Available);
                    if (!copyUpdated.Success)
                    {
                        _response.IsSuccess = false;
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.ErrorMessages.Add(copyUpdated.Message);
                        return BadRequest(_response);
                    }
                }
                

                _db.Borrowings.Update(existingBorrowing);
                await _db.SaveChangesAsync();

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
