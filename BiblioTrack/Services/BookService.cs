using Azure;
using BiblioTrack.Data;
using BiblioTrack.Models;
using BiblioTrack.Models.Dto;
using BiblioTrack.Utility;
using Microsoft.EntityFrameworkCore;

namespace BiblioTrack.Services
{
    public class BookService : IBookService
    {
        private readonly ApplicationDbContext _db;
        public BookService(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<PagedResponse<Book>> GetBooksAsync(GetBooksRequest getBooksRequest)
        {
            try
            {
                IQueryable<Book> query = _db.Book.AsQueryable();

                if (!string.IsNullOrEmpty(getBooksRequest.Title))
                {
                    query = query.Where(b => b.Title.Contains(getBooksRequest.Title));
                }
                if (!string.IsNullOrEmpty(getBooksRequest.Author))
                {
                    query = query.Where(b => b.Author.Contains(getBooksRequest.Author));
                }
                if (!string.IsNullOrEmpty(getBooksRequest.ISBN))
                {
                    query = query.Where(b => b.Author.Contains(getBooksRequest.ISBN));
                }
                if (!string.IsNullOrEmpty(getBooksRequest.Publisher))
                {
                    query = query.Where(b => b.Publisher.Contains(getBooksRequest.Publisher));
                }
                if (!string.IsNullOrEmpty(getBooksRequest.Category))
                {
                    query = query.Where(b => b.Category.Contains(getBooksRequest.Category));
                }
     
                var totalRecords = await query.CountAsync();


                var books = query
                    .Skip((getBooksRequest.PageNumber - 1) * getBooksRequest.PageSize)
                    .Take(getBooksRequest.PageSize)
                    .ToList();


                var response = new PagedResponse<Book>
                {
                    PageNumber = getBooksRequest.PageNumber,
                    PageSize = getBooksRequest.PageSize,
                    TotalRecords = totalRecords,
                    Data = books
                };

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task<PagedResponse<BorrowableBookDto>> GetBorrowableBooksAsync(GetBooksRequest getBooksRequest, string? userId = null)
        {
            try
            {
                var query = _db.Book
                            .Select(b => new BorrowableBookDto
                            {
                                BookId = b.BookId,
                                Title = b.Title,
                                Author = b.Author,
                                ISBN = b.ISBN,
                                Publisher = b.Publisher,
                                Category = b.Category,
                                ImageUrl = b.ImageUrl,
                                TotalCopies = _db.BookCopy.Count(c => c.BookId == b.BookId && c.Status == SD.Book_Copy_Status_Available),
                                IsUserFavorite = getBooksRequest.IncludeUserFavorites && userId != null ? _db.UserFavoriteBook.Any(f => f.BookId == b.BookId && f.UserId == userId) : (bool?)null
                            });


                if (!string.IsNullOrEmpty(getBooksRequest.Title))
                {
                    query = query.Where(b => b.Title.Contains(getBooksRequest.Title));
                }
                if (!string.IsNullOrEmpty(getBooksRequest.Author))
                {
                    query = query.Where(b => b.Author.Contains(getBooksRequest.Author));
                }
                if (!string.IsNullOrEmpty(getBooksRequest.ISBN))
                {
                    query = query.Where(b => b.Author.Contains(getBooksRequest.ISBN));
                }
                if (!string.IsNullOrEmpty(getBooksRequest.Publisher))
                {
                    query = query.Where(b => b.Publisher.Contains(getBooksRequest.Publisher));
                }
                if (!string.IsNullOrEmpty(getBooksRequest.Category))
                {
                    query = query.Where(b => b.Category.Contains(getBooksRequest.Category));
                }
                if (getBooksRequest.GetAvailableOnly)
                {
                    query = query.Where(b => b.TotalCopies > 0);
                }
                var totalRecords = await query.CountAsync();

                var booksWithCopies = query
                    .Skip((getBooksRequest.PageNumber - 1) * getBooksRequest.PageSize)
                    .Take(getBooksRequest.PageSize)
                    .ToList();

                var response = new PagedResponse<BorrowableBookDto>
                {
                    PageNumber = getBooksRequest.PageNumber,
                    PageSize = getBooksRequest.PageSize,
                    TotalRecords = totalRecords,
                    Data = booksWithCopies
                };

                return response;
            }
            catch (Exception ex)
            {
                return new PagedResponse<BorrowableBookDto>
                {
                    PageNumber = getBooksRequest.PageNumber,
                    PageSize = getBooksRequest.PageSize,
                    TotalRecords = 0,
                    Data = new List<BorrowableBookDto>()
                };
            }

        }
        public async Task<BorrowableBookDto> GetBorrowableBookByIdAsync(int bookId, string? userId = null)
        {
            try
            {
                var book = await _db.Book
                    .Where(b => b.BookId == bookId)
                    .Select(b => new BorrowableBookDto
                    {
                        BookId = b.BookId,
                        Title = b.Title,
                        Author = b.Author,
                        ISBN = b.ISBN,
                        Publisher = b.Publisher,
                        Category = b.Category,
                        ImageUrl = b.ImageUrl,
                        Description = b.Description,
                        AverageRating =b.AverageRating,
                        NumPages = b.NumPages,
                        RatingsCount = b.RatingsCount,
                        TotalCopies = _db.BookCopy.Count(c => c.BookId == b.BookId && c.Status == SD.Book_Copy_Status_Available),
                        IsUserFavorite = userId != null ? _db.UserFavoriteBook.Any(f => f.BookId == b.BookId && f.UserId == userId): (bool?)null
                    })
                    .FirstOrDefaultAsync();

                if (book == null)
                {
                    throw new Exception("Book not found");
                }
                return book;
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }


        }
    }
}
