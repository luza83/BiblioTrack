using BiblioTrack.Models.Dto;

namespace BiblioTrack.Services
{
    public interface IBookCopyService
    {
        Task<(bool Success, string Message)> UpdateBookCopy(int copyId, string copyStatus, bool commitChanges = true);

    }
}
