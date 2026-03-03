using BiblioTrack.Models;
using BiblioTrack.Models.Dto;

namespace BiblioTrack.Services
{
    public interface IBookService
    {
        Task <PagedResponse<Book>> GetBooksAsync(GetBooksRequest getBooksRequest);
         //Task<Book> GetBookByIdAsync(int id);
         //Task<(bool Success, string Message)> AddBookAsync(Book book);
         //Task<(bool Success, string Message)> UpdateBookAsync(int id, Book book);
         //Task<(bool Success, string Message)> DeleteBookAsync(int id);>
    }
}
