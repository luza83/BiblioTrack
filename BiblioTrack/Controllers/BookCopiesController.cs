using Azure;
using BiblioTrack.Data;
using BiblioTrack.Models;
using BiblioTrack.Models.Dto;
using BiblioTrack.Services;
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
        private readonly IBookCopyService _bookCopyService;
        private readonly ApiResponse _response;
        private readonly IWebHostEnvironment _env;
        public BookCopiesController(ApplicationDbContext db, IWebHostEnvironment env, IBookCopyService bookCopyService)
        {
            _db = db;
            _bookCopyService = bookCopyService;
            _response = new ApiResponse();
            _env = env;
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
        public IActionResult GetBookCopyByCopyId(int copyId)
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
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ApiResponse>> AddBookCopy([FromBody] BookCopyDTO bookCopyCreateDto)
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
        [Authorize]
        [HttpPut("{bookCopyId:int}", Name = "UpdateBookCopy")]
        public async Task<ActionResult<ApiResponse>> UpdateBookCopy(int bookCopyId, [FromBody] BookCopyUpdateDTO bookCopyUpdateDto)
        {
            try
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
        [Authorize]
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
                var isAdmin = User.IsInRole(SD.Role_Admin);
                if (!isAdmin)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.Forbidden;
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
