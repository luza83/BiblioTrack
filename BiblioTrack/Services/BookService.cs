using Azure;
using BiblioTrack.Data;
using BiblioTrack.Models;
using BiblioTrack.Models.Dto;
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
                return new PagedResponse<Book>
                {
                    PageNumber = getBooksRequest.PageNumber,
                    PageSize = getBooksRequest.PageSize,
                    TotalRecords = 0,
                    Data = new List<Book>()
                };
            }
        }
    }
}
