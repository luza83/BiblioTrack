using BiblioTrack.Models.Dto;

namespace BiblioTrack.Services
{
    public interface IBookCopyService
    {
        Task<(bool Success, string Message)> UpdateBookCopy(int copyId, string copyStatus);
        Task<PagedResponse<BookAndCopiesDTO>> GetBooksWithCopiesAsync(GetBooksRequest getBooksRequest, string? userId = null);

    }
}
