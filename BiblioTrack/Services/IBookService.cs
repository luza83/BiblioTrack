using BiblioTrack.Models;
using BiblioTrack.Models.Dto;

namespace BiblioTrack.Services
{
    public interface IBookService
    {
        Task <PagedResponse<Book>> GetBooksAsync(GetBooksRequest getBooksRequest);
        Task<PagedResponse<BorrowableBookDto>> GetBorrowableBooksAsync(GetBooksRequest getBooksRequest, string? userId = null);
        Task<BorrowableBookDto> GetBorrowableBookByIdAsync(int bookId, string? userId = null);
 
    }
}
