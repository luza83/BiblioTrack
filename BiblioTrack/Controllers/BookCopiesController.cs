using BiblioTrack.Data;
using BiblioTrack.Models;
using BiblioTrack.Models.Dto;
using BiblioTrack.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net;
using System.Security.Claims;

namespace BiblioTrack.Controllers
{
    [Route("api/bookCopies")]
    [ApiController]
    public class BookCopiesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApiResponse _response;
        private readonly IWebHostEnvironment _env;
        public BookCopiesController(ApplicationDbContext db, UserManager<ApplicationUser> userManager ,IWebHostEnvironment env)
        {
            _db = db;
            _userManager = userManager;
            _response = new ApiResponse();
            _env = env;
        }
        [Authorize]
        [HttpGet]
        public IActionResult GetAvailableBooks(bool getAvailableOnly = false)
        {
            List<Book> AllBooks = _db.Book.ToList();
            List<int> BookIds = AllBooks.Select(b => b.BookId).ToList();
            List<BookCopy> BookCopies = _db.BookCopy
                                .Where(bc => BookIds.Contains(bc.BookId))
                                .ToList();
            List<BookAndCopiesDTO> BooksWithCopies = new List<BookAndCopiesDTO>();
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("id")?.Value;


            List<UserFavoriteBookModel> favoriteBooks = new List<UserFavoriteBookModel>();
            foreach (var book in AllBooks)
            {
                List<BookCopy> bookTotalCopies = BookCopies.Where(bc => bc.BookId == book.BookId).ToList();
                if (getAvailableOnly)
                {
                    bookTotalCopies = bookTotalCopies.Where(c => c.Status == SD.Book_Copy_Status_Available).ToList();
                    if (currentUserId != null)
                    {
                        favoriteBooks = _db.UserFavoriteBook.Where(u => u.UserId == currentUserId).ToList();
                       
                    }
                }
                BooksWithCopies.Add(new  BookAndCopiesDTO 
                {
                    BookId = book.BookId,
                    Title = book.Title,
                    Author = book.Author,
                    Publisher = book.Publisher,
                    Category = book.Category,
                    ImageUrl = book.ImageUrl,
                    TotalCopies = bookTotalCopies.Count,
                    IsUserFavorite = getAvailableOnly ? favoriteBooks.Any(b => b.BookId == book.BookId) : null,
                    
                });


            }
            _response.Result = getAvailableOnly ? BooksWithCopies.Where(bc => bc.TotalCopies > 0).ToList() : BooksWithCopies;
            _response.IsSuccess = true;
            _response.StatusCode = System.Net.HttpStatusCode.OK;
            return Ok(_response);
        }

        [HttpGet("copies/{bookId:int}", Name = "GetBookCopies")]
        public IActionResult GetBookCopies(int bookId)
        {
            if (bookId == 0)
            {
                _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return Ok(_response);
            }
            _response.Result = _db.BookCopy
                               .Where(bc => bc.BookId == bookId)
                               .ToList();
            _response.IsSuccess = true;
            _response.StatusCode = System.Net.HttpStatusCode.OK;
            return Ok(_response);
        }


        [HttpGet("{copyId:int}", Name = "GetBookCopyById")]
        public IActionResult GetBookCopyById(int copyId)
        {
            if (copyId == 0)
            {
                _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return Ok(_response);
            }
            _response.Result = _db.BookCopy
                               .Where(bc => bc.CopyId == copyId)
                               .ToList();
            _response.StatusCode = System.Net.HttpStatusCode.OK;
            return Ok(_response);
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse>> AddBookCopy([FromBody] BookCopyDTO bookCopyCreateDto)
        {
            if (!ModelState.IsValid)
            {
                _response.IsSuccess = false;
                return BadRequest(_response);
            }

            if (bookCopyCreateDto.BookId == 0  || bookCopyCreateDto.Status.Length == 0)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages = ["Missing Book Details"];
                return BadRequest(_response);
            }

            try
            {

                BookCopy bookCopy = new()
                {
                    BookId = bookCopyCreateDto.BookId,
                    Status = bookCopyCreateDto.Status,
                    Location = bookCopyCreateDto.Location,

                };

                _db.BookCopy.Add(bookCopy);
                await _db.SaveChangesAsync();

                _response.Result = bookCopyCreateDto;
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

        [HttpPut("{bookCopyId:int}", Name = "UpdateBookCopy")]
        public async Task<ActionResult<ApiResponse>> UpdateBookCopy(int bookCopyId, [FromBody] BookCopyDTO bookCopyUpdateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    return BadRequest(_response);
                }

                BookCopy? existingBookCopy = _db.BookCopy.FirstOrDefault(u => u.CopyId == bookCopyId);

                if (existingBookCopy == null || bookCopyUpdateDto == null || existingBookCopy?.CopyId != bookCopyId)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }



                if (!string.IsNullOrWhiteSpace(bookCopyUpdateDto.Status) &&
                    existingBookCopy.Status != bookCopyUpdateDto.Status)
                {
                    existingBookCopy.Status = bookCopyUpdateDto.Status;
                }

                if (!string.IsNullOrWhiteSpace(bookCopyUpdateDto.Location) &&
                    existingBookCopy.Location != bookCopyUpdateDto.Location)
                {
                    existingBookCopy.Location = bookCopyUpdateDto.Location;
                }

                _db.BookCopy.Update(existingBookCopy);
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

        [HttpDelete]
        public async Task<ActionResult<ApiResponse>> DeleteBookCopy(int bookCopyId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    return BadRequest(_response);

                }

                if (bookCopyId == 0)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                BookCopy? exisitingBookCopy = await _db.BookCopy.FirstOrDefaultAsync(u => u.CopyId == bookCopyId);

                if (exisitingBookCopy == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                _db.BookCopy.Remove(exisitingBookCopy);
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
