using BiblioTrack.Data;
using BiblioTrack.Models;
using BiblioTrack.Models.Dto;
using BiblioTrack.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BiblioTrack.Services
{
    public class BookCopyService : IBookCopyService
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookCopyService(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<(bool Success, string Message)> UpdateBookCopy(int copyId, string copyStatus)
        {
            var existingBookCopy = await _db.BookCopy
                .FirstOrDefaultAsync(u => u.CopyId == copyId);

            if (existingBookCopy == null)
                return (false, "Book Copy not found");

            try
            {
                if(existingBookCopy.Status != SD.Book_Copy_Status_Available && copyStatus == SD.Book_Copy_Status_Borrowed)
                {
                    return (false, "Book is not available");
                }
                existingBookCopy.Status = copyStatus;
                _db.BookCopy.Update(existingBookCopy);
                await _db.SaveChangesAsync();
                return (true, "Book copy successfully updated");
            }
            catch (Exception)
            {
                return (false, "Failed to update book copy" );
            }
        }
        public async Task<PagedResponse<BookAndCopiesDTO>> GetBooksWithCopiesAsync(GetBooksRequest getBooksRequest, string? userId = null)
        {
            try
            {


                var query = _db.Book
                            .Select(b => new BookAndCopiesDTO
                            {
                                BookId = b.BookId,
                                Title = b.Title,
                                Author = b.Author,
                                ISBN = b.ISBN,
                                Publisher = b.Publisher,
                                Category = b.Category,
                                ImageUrl = b.ImageUrl,
                                TotalCopies = _db.BookCopy.Count(c => c.BookId == b.BookId),
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
                var totalRecords = await query.CountAsync();

                var booksWithCopies = query
                    .Skip((getBooksRequest.PageNumber - 1) * getBooksRequest.PageSize)
                    .Take(getBooksRequest.PageSize)
                    .ToList();

                var response = new PagedResponse<BookAndCopiesDTO>
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
                return new PagedResponse<BookAndCopiesDTO>
                {
                    PageNumber = getBooksRequest.PageNumber,
                    PageSize = getBooksRequest.PageSize,
                    TotalRecords = 0,
                    Data = new List<BookAndCopiesDTO>()
                };
            }
                      
        }
    }
}
