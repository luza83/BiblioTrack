using BiblioTrack.Data;
using BiblioTrack.Models;
using BiblioTrack.Models.Dto;
using BiblioTrack.Services;
using BiblioTrack.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;


namespace BiblioTrack.Controllers
{
    [Route("api/books")]
    [ApiController]
    public class BookController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ApiResponse _response;
        private readonly IBookService _bookservice;
        private readonly IWebHostEnvironment _env;
        public BookController(ApplicationDbContext db, IWebHostEnvironment env, IBookService bookService)
        {
            _db = db;
            _response = new ApiResponse();
            _env = env;
            _bookservice = bookService;
        }

        [HttpGet]
        public async Task<IActionResult> GetBooks([FromQuery] GetBooksRequest getBooksRequest)
        {
            if (!ModelState.IsValid)
            {
                _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }

            var response = await _bookservice.GetBooksAsync(getBooksRequest);

            if (response == null)
            {
                _response.StatusCode = System.Net.HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                return NotFound(_response);
            }
            _response.Result = response;
            _response.IsSuccess = true;
            _response.StatusCode = System.Net.HttpStatusCode.OK;
            return Ok(_response);
        }


        [HttpGet("{id:int}", Name ="GetBookById")]
        public IActionResult GetBookById(int id)
        {
            if (id == 0)
            {
                _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return Ok(_response);
            }
            Book? book = _db.Book.FirstOrDefault(u=> u.BookId == id);
            _response.Result = book;
            _response.IsSuccess = true;
            _response.StatusCode = System.Net.HttpStatusCode.OK;
            return Ok(_response);
        }
        
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ApiResponse>> CreateBook([FromForm] BookCreateDto bookCreateDto)
        {
            if (!ModelState.IsValid)
            {
                _response.IsSuccess = false;
                return BadRequest(_response);
            }
            var isAdmin = User.IsInRole(SD.Role_Admin);
            if (!isAdmin)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.Forbidden;
                return BadRequest(_response);
            }
            try
            {
 
                Book book = new()
                {
                    Title = bookCreateDto.Title,
                    Author = bookCreateDto.Author,
                    ISBN = bookCreateDto.ISBN,
                    Publisher = bookCreateDto.Publisher,
                    Category = bookCreateDto.Category,
                    CreatedAt = DateTime.Now,
                    ImageUrl = bookCreateDto.ImageUrl,
                    Description = bookCreateDto.Description,
                    NumPages = bookCreateDto.NumPages
                };

                _db.Book.Add(book);
                await _db.SaveChangesAsync();

                _response.Result = bookCreateDto;
                _response.StatusCode = HttpStatusCode.Created;
                _response.IsSuccess = true;
                return CreatedAtRoute("GetBookById", new { id = book.BookId }, _response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages
                     = [ex.ToString()];
                return BadRequest(_response);
            }
            
        }
        [Authorize]
        [HttpPut("{bookId:int}", Name = "UpdateBook")]
        public async Task<ActionResult<ApiResponse>> UpdateBook(int bookId, [FromForm] BookUpdateDto bookUpdateDto)
        {
            if (!ModelState.IsValid)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;

            }
            var isAdmin = User.IsInRole(SD.Role_Admin);
            if (!isAdmin)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.Forbidden;
                return BadRequest(_response);
            }

            if (bookUpdateDto == null || bookUpdateDto.BookId != bookId)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }

            try
            {

                Book? existingBook = _db.Book.FirstOrDefault(u => u.BookId == bookId);

                if (existingBook == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                if (!string.IsNullOrWhiteSpace(bookUpdateDto.Title) &&
                    existingBook.Title != bookUpdateDto.Title)
                {
                    existingBook.Title = bookUpdateDto.Title;
                }

                if (!string.IsNullOrWhiteSpace(bookUpdateDto.Author) &&
                    existingBook.Author != bookUpdateDto.Author)
                {
                    existingBook.Author = bookUpdateDto.Author;
                }

                if (!string.IsNullOrWhiteSpace(bookUpdateDto.ISBN) &&
                    existingBook.ISBN != bookUpdateDto.ISBN)
                {
                    existingBook.ISBN = bookUpdateDto.ISBN;
                }

                if (!string.IsNullOrWhiteSpace(bookUpdateDto.Publisher) &&
                    existingBook.Publisher != bookUpdateDto.Publisher)
                {
                    existingBook.Publisher = bookUpdateDto.Publisher;
                }

                if (!string.IsNullOrWhiteSpace(bookUpdateDto.Category) &&
                    existingBook.Category != bookUpdateDto.Category)
                {
                    existingBook.Category = bookUpdateDto.Category;
                }

                if (bookUpdateDto.CreatedAt != default &&
                    existingBook.CreatedAt != bookUpdateDto.CreatedAt)
                {
                    existingBook.CreatedAt = bookUpdateDto.CreatedAt;
                }



                if (!string.IsNullOrEmpty(bookUpdateDto.ImageUrl) && existingBook.ImageUrl != bookUpdateDto.ImageUrl)
                {
                    existingBook.ImageUrl = bookUpdateDto.ImageUrl;

                }

                if(!string.IsNullOrEmpty(bookUpdateDto.Description) && existingBook.Description != bookUpdateDto.Description)
                {
                    existingBook.Description = bookUpdateDto.Description;
                }
                if(bookUpdateDto.NumPages > 0 && bookUpdateDto.NumPages != existingBook.NumPages)
                {
                    existingBook.NumPages = bookUpdateDto.NumPages;
                }

                _db.Book.Update(existingBook);
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
        [Authorize]
        [HttpDelete]
        public async Task<ActionResult<ApiResponse>> DeleteBook(int bookId)
        {
            try
            {
                if (! ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    return BadRequest(_response);

                }
                var isAdmin = User.IsInRole(SD.Role_Admin);
                if (!isAdmin)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.Forbidden;
                    return BadRequest(_response);
                }

                if (bookId == 0)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                Book? existingBook = await _db.Book.FirstOrDefaultAsync(u => u.BookId == bookId);

                if (existingBook == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }


                _db.Book.Remove(existingBook);
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
            }

            return BadRequest(_response);
        }
        
        [Authorize]
        [HttpGet("borrowable")]
        public async Task<IActionResult> GetAvailableBooks([FromQuery] GetBooksRequest getBooksRequest)
        {
            if (!ModelState.IsValid)
            {
                _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }
            string? currentUserId = null;
            if (getBooksRequest.IncludeUserFavorites)
            {
                currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("id")?.Value;

            }
            var response = await _bookservice.GetBorrowableBooksAsync(getBooksRequest, currentUserId);
            if (response == null)
            {
                _response.StatusCode = System.Net.HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                return NotFound(_response);
            }
            _response.Result = response;
            _response.IsSuccess = true;
            _response.StatusCode = System.Net.HttpStatusCode.OK;
            return Ok(_response);
        }

        [HttpGet("borrowable/{bookId:int}", Name = "GetBorrowableBookById")]
        public async Task<IActionResult> GetBorrowableBookById(int bookId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("id")?.Value;

            if (bookId == 0)
            {
                _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return Ok(_response);
            }
            var result = await _bookservice.GetBorrowableBookByIdAsync(bookId, currentUserId);
            _response.Result = result;
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }



    }
}