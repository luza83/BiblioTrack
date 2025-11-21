using BiblioTrack.Data;
using BiblioTrack.Models;
using BiblioTrack.Models.Dto;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;


namespace BiblioTrack.Controllers
{
    [Route("api/book")]
    [ApiController]
    public class BookController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly ApiResponse _response;
        private readonly IWebHostEnvironment _env;
        public BookController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _response = new ApiResponse();
            _env = env;
        }

        [HttpGet]
        public IActionResult GetBooks()
        {
            _response.Result=_db.Book.ToList();
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
            _response.StatusCode = System.Net.HttpStatusCode.OK;
            return Ok(_response);
        }
       
        [HttpPost]
        public async Task<ActionResult<ApiResponse>> CreateBook([FromForm] BookCreateDto bookCreateDto)
        {
            if (!ModelState.IsValid)
            {
                _response.IsSuccess = false;
                return BadRequest(_response);
            }

            if (bookCreateDto.ImageFile == null || bookCreateDto.ImageFile.Length == 0)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.ErrorMessages = ["File is required"];
                return BadRequest(_response);
            }

            try
            {
                var imagesPath = Path.Combine(_env.WebRootPath, "images");
                if (!Directory.Exists(imagesPath))
                {
                    Directory.CreateDirectory(imagesPath);
                }
                var filePath = Path.Combine(imagesPath, bookCreateDto.ImageFile.FileName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                //uploading the image
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await bookCreateDto.ImageFile.CopyToAsync(stream);
                }

                Book book = new()
                {
                    Title = bookCreateDto.Title,
                    Author = bookCreateDto.Author,
                    ISBN = bookCreateDto.ISBN,
                    Publisher = bookCreateDto.Publisher,
                    Category = bookCreateDto.Category,
                    CreatedAt = new DateTime(),
                    ImageUrl = "images/" + bookCreateDto.ImageFile.FileName
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
        
        [HttpPut]
        public async Task<ActionResult<ApiResponse>> UpdateBook(int bookId, [FromForm] BookUpdateDto bookUpdateDto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (bookUpdateDto == null || bookUpdateDto.BookId != bookId)
                    {
                        _response.IsSuccess = false;
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        return BadRequest(_response);
                    }

                    Book? existingBook =  _db.Book.FirstOrDefault(u => u.BookId == bookId);

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



                    if (bookUpdateDto.ImageFile != null && bookUpdateDto.ImageFile.Length > 0)
                    {
                        var imagesPath = Path.Combine(_env.WebRootPath, "images");
                        if (!Directory.Exists(imagesPath))
                        {
                            Directory.CreateDirectory(imagesPath);
                        }
                        var filePath = Path.Combine(imagesPath, bookUpdateDto.ImageFile.FileName);
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                        var filePath_OldFile = Path.Combine(_env.WebRootPath, existingBook.ImageUrl);
                        if (System.IO.File.Exists(filePath_OldFile))
                        {
                            System.IO.File.Delete(filePath_OldFile);
                        }
                        //uploading the image
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await bookUpdateDto.ImageFile.CopyToAsync(stream);
                        }
                        existingBook.ImageUrl = "images/" + bookUpdateDto.ImageFile.FileName;
                    }

                    _db.Book.Update(existingBook);
                    await _db.SaveChangesAsync();

                    _response.StatusCode = HttpStatusCode.NoContent;
                    return Ok(_response);

                }
                else
                {
                    _response.IsSuccess = false;
                }
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
        public async Task<ActionResult<ApiResponse>> DeleteBook(int bookId)
        {
            try
            {
                if (! ModelState.IsValid)
                {
                    _response.IsSuccess = false;
                    return BadRequest(_response);

                }

                if (bookId == 0)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                Book? menuItemFromDb = await _db.Book.FirstOrDefaultAsync(u => u.BookId == bookId);

                if (menuItemFromDb == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                var filePath_OldFile = Path.Combine(_env.WebRootPath, menuItemFromDb.ImageUrl);
                if (System.IO.File.Exists(filePath_OldFile))
                {
                    System.IO.File.Delete(filePath_OldFile);
                }
                _db.Book.Remove(menuItemFromDb);
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
    }
}